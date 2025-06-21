namespace Infrastructure.DbClasses;

public class CompanyDb
{
    public int Id { get; set; } // Changed from string to int
    public string Name { get; set; } = string.Empty;
    public string? ParentCompanyId { get; set; }
}