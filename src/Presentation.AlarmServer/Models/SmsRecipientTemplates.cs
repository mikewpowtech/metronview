namespace Presentation.AlarmServer.Models;

public record SmsRecipientTemplates
{
    public string Sms { get; init; }
    public string ToAddressTemplate { get; init; }
    public string SubjectTemplate { get; init; }
    public string BodyTemplate { get; init; }
    public string FromAddress { get; init; }
    public int CompanyId { get; init; }
    public int RecipientId { get; init; }
}