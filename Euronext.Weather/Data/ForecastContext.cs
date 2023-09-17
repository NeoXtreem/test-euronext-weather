using Euronext.Weather.Models;
using Microsoft.EntityFrameworkCore;

namespace Euronext.Weather.Data;

public class ForecastContext : DbContext
{
    public ForecastContext(DbContextOptions options) : base(options) { }

    public DbSet<Forecast> Forecasts { get; set; } = null!;
}