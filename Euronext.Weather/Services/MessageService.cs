using Microsoft.Extensions.Localization;

namespace Euronext.Weather.Services;

public static class LocalizerService
{
    public static IStringLocalizer Localizer { get; set; } = null!;

    public static string GetPastDatesErrorMessage() => Localizer["PastDatesErrorMessage"];

    internal static string? GetTemperatureOutOfRangeErrorMessage() => Localizer["TemperatureOutOfRangeErrorMessage"];
}