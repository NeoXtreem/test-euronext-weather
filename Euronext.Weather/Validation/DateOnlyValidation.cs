namespace Euronext.Weather.Validation;

public sealed class DateOnlyValidation
{
    public static bool IsTodayOrLater(DateOnly date) => date >= DateOnly.FromDateTime(DateTime.Today);
}