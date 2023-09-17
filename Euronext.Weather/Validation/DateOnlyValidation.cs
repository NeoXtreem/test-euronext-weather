namespace Euronext.Weather.Validation;

public class DateOnlyValidation
{
    public static bool IsTodayOrLater(DateOnly date) => date >= DateOnly.FromDateTime(DateTime.Today);
}