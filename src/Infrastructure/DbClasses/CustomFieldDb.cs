using Domain.Enums;

namespace Infrastructure.DbClasses;

public class CustomFieldDb
{
    public int Id { get; set; }
    public int ForeignKeyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public CustomFieldType CustomFieldType { get; set; }
}