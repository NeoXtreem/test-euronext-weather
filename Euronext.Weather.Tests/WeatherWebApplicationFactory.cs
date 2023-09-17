using Euronext.Weather.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Euronext.Weather.Tests;

public class WeatherWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Remove(services.Single(d => d.ServiceType == typeof(DbContextOptions<ForecastContext>)));
            services.AddDbContext<ForecastContext>(options => options.UseInMemoryDatabase(_databaseName));
        });

        builder.UseEnvironment("Development");
    }
}