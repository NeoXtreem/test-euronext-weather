namespace Euronext.Weather.Helpers;

internal sealed class DateOnlyValidationHelper
{
    public static bool IsTodayOrLater(DateOnly date) => date >= DateOnly.FromDateTime(DateTime.Today);
}