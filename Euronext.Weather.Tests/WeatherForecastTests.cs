using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoFixture;
using Euronext.Weather.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Euronext.Weather.Tests;

public class WeatherForecastTests
{
    private readonly Fixture _fixture = new();
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly IConfiguration _configuration = new ConfigurationBuilder().AddUserSecrets<WeatherForecastTests>().Build();


    [Theory]
    [InlineData(Population.Empty)]
    [InlineData(Population.Partial)]
    [InlineData(Population.Full)]
    public async Task GivenWeatherForecastsAdded_WhenWeekWeatherForecastRequested_ShouldReturnWeekWeatherForecast(Population forecastsPopulation)
    {
        // Arrange
        _fixture.Customizations.Add(new RandomNumericSequenceGenerator(1, 6));

        var daysOfWeekToForecast = forecastsPopulation switch
        {
            Population.Full => Enumerable.Range(0, 7),
            Population.Partial => _fixture.CreateMany<int>(),
            _ => Enumerable.Empty<int>()
        };

        var weekForecasts = daysOfWeekToForecast.Select(d => new WeatherForecast(DateOnly.FromDateTime(DateTime.Today).AddDays(d), 1, String.Empty));

        using (var client = GetClient("Weatherman"))
        {
            foreach (var forecast in weekForecasts)
            {
                var response = await client.PostAsJsonAsync("/weatherforecast", forecast);
                response.EnsureSuccessStatusCode();
            }
        }

        using (var client = GetClient("Reader"))
        {
            // Act
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var response = await client.GetAsync($"/weekweatherforecast?startDate={startDate:O}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>();

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