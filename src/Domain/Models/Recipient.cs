namespace Domain;

public class Recipient
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? UnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Sms { get; set; } = string.Empty;
    public string WebServiceRoot { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}
