using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoFixture;
using Euronext.Weather.Models;
using Microsoft.Extensions.Configuration;

namespace Euronext.Weather.Tests;

public class WeatherForecastTests
{
    private readonly Fixture _fixture = new();
    private readonly WeatherWebApplicationFactory _factory = new();
    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddJsonFile($"appsettings.json")
        .AddJsonFile($"appsettings.Development.json")
        .AddUserSecrets<WeatherForecastTests>().Build();

    public WeatherForecastTests()
    {
        _fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));
    }

    [Theory]
    [InlineData(Population.Empty)]
    [InlineData(Population.Partial)]
    [InlineData(Population.Full)]
    public async Task GivenWeatherForecastsAdded_WhenWeekWeatherForecastRequested_ShouldReturnWeekWeatherForecast(Population forecastsPopulation)
    {
        // Arrange
        var partialWeekDaysLimitsGenerator = new RandomNumericSequenceGenerator(1, 6);
        _fixture.Customizations.Add(partialWeekDaysLimitsGenerator);

        // Set up the number of days that will be forecast in the week that the user will request.
        var daysOfWeekToForecast = forecastsPopulation switch
        {
            Population.Full => Enumerable.Range(0, 7), // The whole week will be forecast
            Population.Partial => _fixture.CreateMany<int>(), // Part of the week will be forecast.
            _ => Enumerable.Empty<int>() // No days in the week will be forecast.
        };

        _fixture.Customizations.Remove(partialWeekDaysLimitsGenerator);

        // Set the date the user will request to a random date after today.
        var startDate = DateOnly.FromDateTime(DateTime.Today).AddDays(_fixture.Create<int>());

        // Create some forecasts for the week (per the required days) ensuring the temperature is within the configured limits.
        var weatherForecastingConfig = _configuration.GetSection("WeatherForecasting");
        _fixture.Customizations.Add(new RandomNumericSequenceGenerator(weatherForecastingConfig.GetValue<long>("MinTemperature"), weatherForecastingConfig.GetValue<long>("MaxTemperature")));
        var weekForecasts = daysOfWeekToForecast.Select(d => new WeatherForecast(startDate.AddDays(d), _fixture.Create<int>(), _fixture.Create<string>())).ToArray();

        using (var client = GetClient("Weatherman"))
        {
            var totalForecasts = weekForecasts.ToList();

            // Add some forecasts before and after the week that the user will request.
            foreach (var (min, max) in new (DateOnly Min, DateOnly Max) [] { (DateOnly.MinValue, startDate.AddDays(-1)), (startDate.AddDays(8), DateOnly.MaxValue) })
            {
                var dateLimitsGenerator = new RandomDateTimeSequenceGenerator(min.ToDateTime(TimeOnly.MinValue), max.ToDateTime(TimeOnly.MinValue));
                _fixture.Customizations.Add(dateLimitsGenerator);
                totalForecasts.AddRange(_fixture.CreateMany<WeatherForecast>());
                _fixture.Customizations.Remove(dateLimitsGenerator);
            }

            // Add all the forecasts to the weather service.
            foreach (var forecast in totalForecasts)
            {
                var response = await client.PostAsJsonAsync("/weatherforecast", forecast);
                response.EnsureSuccessStatusCode();
            }
        }

        using (var client = GetClient("Reader"))
        {
            // Act
            var response = await client.GetAsync($"/weekweatherforecast?startDate={startDate:O}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();

            Assert.Equivalent(weekForecasts, result);
        }
    }

    private HttpClient GetClient(string apiKeyName)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration[$"Weather:{apiKeyName}ApiKey"]);
        return client;
    }

    public enum Population
    {
        Empty,
        Partial,
        Full
    }
}