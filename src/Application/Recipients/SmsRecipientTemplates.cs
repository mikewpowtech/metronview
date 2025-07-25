namespace Application.Recipients
{
    public class RecipientTemplates
    {
        public string Sms { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ToAddressTemplate { get; set; }
        public string? SubjectTemplate { get; set; }
        public string? BodyTemplate { get; set; }
        public string? FromAddress { get; set; }
        public string? ReplyToAddress { get; set; }
        public int CompanyId { get; set; }
        public int RecipientId { get; set; }
    }
}