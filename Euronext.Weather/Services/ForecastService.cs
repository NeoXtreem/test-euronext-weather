using Euronext.Weather.Data;
using Euronext.Weather.Models;

namespace Euronext.Weather.Services;

internal sealed class ForecastService
{
    private readonly ForecastContext _forecastDb;

    public ForecastService(ForecastContext storeContext)
    {
        _forecastDb = storeContext;
    }

    public void AddForecast(Forecast forecast)
    {
        _forecastDb.Forecasts.Add(forecast);
        _forecastDb.SaveChanges();
    }

    public IReadOnlyCollection<Forecast> GetWeekForecast(DateOnly startDate)
    {
        return _forecastDb.Forecasts.Where(f => f.Date >= startDate && f.Date < startDate.AddDays(7)).ToList().AsReadOnly();
    }
}