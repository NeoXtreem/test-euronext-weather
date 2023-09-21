using Microsoft.Extensions.Localization;

namespace Euronext.Weather.Services;

internal static class LocalizerService
{
    public static IStringLocalizer Localizer { get; set; } = null!;

    public static string GetPastDatesErrorMessage() => Localizer["PastDatesErrorMessage"];

    public static string GetTemperatureOutOfRangeErrorMessage() => Localizer["TemperatureOutOfRangeErrorMessage"];
}