using Euronext.Weather.Data;
using Euronext.Weather.Models;

namespace Euronext.Weather.Services;

public class WeatherForecastService
{
    private readonly WeatherForecastContext _weatherForecastDb;

    public WeatherForecastService(WeatherForecastContext storeContext)
    {
        _weatherForecastDb = storeContext;
    }

    public void AddWeatherForecast(WeatherForecast weatherForecast)
    {
        _weatherForecastDb.WeatherForecasts.Add(weatherForecast);
        _weatherForecastDb.SaveChanges();
    }

    public IReadOnlyCollection<WeatherForecast> GetWeekWeatherForecast(DateOnly startDate)
    {
        return _weatherForecastDb.WeatherForecasts.Where(f => f.Date >= startDate && f.Date < startDate.AddDays(7)).ToList().AsReadOnly();
    }
}