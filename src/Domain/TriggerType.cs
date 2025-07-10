
using Domain.Enums;
namespace Domain;

public class TriggerType
{
    public int Id { get; set; } // [Id] [int] NOT NULL, auto-increment primary key
    public TriggerTypeCode Code { get; set; } // [Code] [char] or [int] depending on enum mapping
    public string Name { get; set; } = string.Empty; // [Name] [varchar](100) NOT NULL
    public int Order { get; set; } // [Order] [int] NOT NULL
    // Navigation property: List of triggers of this type
    public ICollection<Trigger>? Triggers { get; set; }
}