using Euronext.Weather.Models;
using Microsoft.EntityFrameworkCore;

namespace Euronext.Weather.Data;

public class WeatherForecastContext : DbContext
{
    public WeatherForecastContext(DbContextOptions options) : base(options) { }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; } = null!;
}