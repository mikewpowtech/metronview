namespace Domain;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ParentCompanyId { get; set; }
}