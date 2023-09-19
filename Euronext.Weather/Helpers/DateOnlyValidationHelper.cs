namespace Euronext.Weather.Helpers;

public sealed class DateOnlyValidationHelper
{
    public static bool IsTodayOrLater(DateOnly date) => date >= DateOnly.FromDateTime(DateTime.Today);
}