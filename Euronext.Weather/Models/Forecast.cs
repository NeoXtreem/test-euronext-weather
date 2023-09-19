using Euronext.Weather.Helpers;
using Euronext.Weather.Services;
using Microsoft.EntityFrameworkCore;

namespace Euronext.Weather.Models;

[PrimaryKey(nameof(Date))]
public sealed record Forecast(DateOnly Date, int TemperatureC, string Summary)
{
    private readonly int _temperatureC = ValidatedTemperatureC(TemperatureC);

    public int TemperatureC
    {
        get => _temperatureC;
        init => _temperatureC = ValidatedTemperatureC(value);
    }

    private static int ValidatedTemperatureC(int temperatureC)
    {
        var options = AppSettingsHelper.Configuration.GetSection("Options");
        var minTemperature = options.GetValue<int>("MinTemperature");
        var maxTemperature = options.GetValue<int>("MaxTemperature");

        if (temperatureC < minTemperature || temperatureC > maxTemperature)
        {
            throw new ArgumentOutOfRangeException(nameof(temperatureC), temperatureC, string.Format(LocalizerService.GetTemperatureOutOfRangeErrorMessage(), minTemperature, maxTemperature));
        }

        return temperatureC;
    }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}