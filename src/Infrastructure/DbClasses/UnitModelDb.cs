namespace Infrastructure.DbClasses;

public class UnitModelDb
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // char(4), NOT NULL
    public string Name { get; set; } = string.Empty; // char(4), NOT NULL
    public string? Description { get; set; } // char(4), NULL
}
