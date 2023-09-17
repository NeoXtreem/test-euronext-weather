using Microsoft.EntityFrameworkCore;

namespace Euronext.Weather.Models;

[PrimaryKey(nameof(Date))]
public sealed record Forecast(DateOnly Date, int TemperatureC, string Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}