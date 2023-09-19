using Euronext.Weather.Data;
using Euronext.Weather.Models;

namespace Euronext.Weather.Services;

public sealed class ForecastService
{
    private readonly ForecastContext _forecastDb;
    private readonly IConfiguration _configuration;

    public ForecastService(ForecastContext storeContext, IConfiguration configuration)
    {
        _forecastDb = storeContext;
        _configuration = configuration;
    }

    public void AddForecast(Forecast forecast)
    {
        var options = _configuration.GetSection("Options");
        var minTemperature = options.GetValue<int>("MinTemperature");
        var maxTemperature = options.GetValue<int>("MaxTemperature");

        if (forecast.TemperatureC < minTemperature || forecast.TemperatureC > maxTemperature)
        {
            throw new ArgumentOutOfRangeException(nameof(forecast), forecast, string.Format(LocalizerService.GetTemperatureOutOfRangeErrorMessage(), minTemperature, maxTemperature));
        }

        _forecastDb.Forecasts.Add(forecast);
        _forecastDb.SaveChanges();
    }

    public IReadOnlyCollection<Forecast> GetWeekForecast(DateOnly startDate)
    {
        return _forecastDb.Forecasts.Where(f => f.Date >= startDate && f.Date < startDate.AddDays(7)).ToList().AsReadOnly();
    }
}