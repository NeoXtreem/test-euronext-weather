using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Resources;
using AutoFixture;
using Euronext.Weather.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Euronext.Weather.Tests;

public sealed class ForecastTests
{
    private readonly Fixture _fixture = new();
    private readonly WeatherWebApplicationFactory _factory = new();
    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.Development.json")
        .AddUserSecrets<ForecastTests>().Build();

    public ForecastTests()
    {
        _fixture.Customize<DateOnly>(c => c.FromFactory<DateTime>(DateOnly.FromDateTime));
    }

    [Theory]
    [InlineData(Population.Empty)]
    [InlineData(Population.Partial)]
    [InlineData(Population.Full)]
    public async Task GivenZerOrMoreForecastsAdded_WhenWeekForecastRequested_ShouldReturnWeekForecast(Population forecastsPopulation)
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
        var forecastsConfig = _configuration.GetSection("Options");
        _fixture.Customizations.Add(new RandomNumericSequenceGenerator(forecastsConfig.GetValue<long>("MinTemperature"), forecastsConfig.GetValue<long>("MaxTemperature")));
        var weekForecasts = daysOfWeekToForecast.Select(d => new Forecast(startDate.AddDays(d), _fixture.Create<int>(), _fixture.Create<string>())).ToArray();

        using (var client = GetClient("Weatherman"))
        {
            var totalForecasts = weekForecasts.ToList();

            // Add some forecasts before and after the week that the user will request.
            foreach (var (min, max) in new (DateOnly Min, DateOnly Max) [] { (DateOnly.FromDateTime(DateTime.Today), startDate.AddDays(-1)), (startDate.AddDays(8), DateOnly.MaxValue) })
            {
                var dateLimitsGenerator = new RandomDateTimeSequenceGenerator(min.ToDateTime(TimeOnly.MinValue), max.ToDateTime(TimeOnly.MinValue));
                _fixture.Customizations.Add(dateLimitsGenerator);
                totalForecasts.AddRange(_fixture.CreateMany<Forecast>().DistinctBy(x => x.Date));
                _fixture.Customizations.Remove(dateLimitsGenerator);
            }

            // Add all the forecasts to the weather service.
            foreach (var forecast in totalForecasts)
            {
                var response = await client.PostAsJsonAsync("/forecast", forecast);
                response.EnsureSuccessStatusCode();
            }
        }

        using (var client = GetClient("Reader"))
        {
            // Act
            var response = await client.GetAsync($"/weekforecast/{startDate:O}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<Forecast[]>();

            Assert.Equivalent(weekForecasts, result);
        }
    }

    [Fact]
    public async Task GivenAPastForecast_WhenForecastAdded_ShouldReturnProblem()
    {
        // Arrange
        using var client = GetClient("Weatherman");
        _fixture.Customizations.Add(new RandomDateTimeSequenceGenerator(DateTime.MinValue, DateTime.Today.AddDays(-1)));

        // Act
        var response = await client.PostAsJsonAsync("/forecast", _fixture.Create<Forecast>());

        // Assert
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(GetResource("PastDatesErrorMessage"), result?.Detail);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GivenAnInvalidTemperature_WhenForecastAdded_ShouldReturnError(bool high)
    {
        // Arrange
        using var client = GetClient("Weatherman");
        var forecastsConfig = _configuration.GetSection("Options");
        _fixture.Customizations.Add(new RandomNumericSequenceGenerator(high ? forecastsConfig.GetValue<long>("MinTemperature") : int.MinValue, high ? int.MaxValue : forecastsConfig.GetValue<long>("MaxTemperature")));
        _fixture.Customizations.Add(new RandomDateTimeSequenceGenerator(DateTime.Today, DateTime.MaxValue));

        // Act
        var response = await client.PostAsJsonAsync("/forecast", _fixture.Create<Forecast>());

        // Assert
        var result = await response.Content.ReadAsStringAsync();
        var options = _configuration.GetSection("Options");
        Assert.Contains(string.Format(GetResource("TemperatureOutOfRangeErrorMessage"), options.GetValue<int>("MinTemperature"), options.GetValue<int>("MaxTemperature")), result);
    }

    [Fact]
    public async Task GivenAPastForecast_WhenWeekForecastRequested_ShouldReturnProblem()
    {
        // Arrange
        using var client = GetClient("Reader");
        _fixture.Customizations.Add(new RandomDateTimeSequenceGenerator(DateTime.MinValue, DateTime.Today.AddDays(-1)));

        // Act
        var response = await client.GetAsync($"/weekforecast/{_fixture.Create<DateOnly>():O}");

        // Assert
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(GetResource("PastDatesErrorMessage"), result?.Detail);
    }

    private HttpClient GetClient(string apiKeyName)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration[$"Weather:{apiKeyName}ApiKey"]);
        return client;
    }

    private static string GetResource(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetManifestResourceNames().Single());
        using var resourceReader = new ResourceReader(stream!);
        resourceReader.GetResourceData(name, out _, out var data);
        using var reader = new BinaryReader(new MemoryStream(data));
        return reader.ReadString();
    }

    public enum Population
    {
        Empty,
        Partial,
        Full
    }
}