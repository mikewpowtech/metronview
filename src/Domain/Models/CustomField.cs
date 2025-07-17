using Domain.Enums;

namespace Domain;

public class CustomField
{
    public int Id { get; set; }
    public int ForeignKeyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public CustomFieldType CustomFieldType { get; set; }
}