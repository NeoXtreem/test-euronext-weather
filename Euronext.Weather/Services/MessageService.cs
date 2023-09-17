using Microsoft.Extensions.Localization;

namespace Euronext.Weather.Services;

public sealed class MessageService
{
    private readonly IStringLocalizer<MessageService> _localizer;

    public MessageService(IStringLocalizer<MessageService> localizer) => _localizer = localizer;

    public string? GetPastDatesErrorMessage() => _localizer["PastDatesErrorMessage"];
}