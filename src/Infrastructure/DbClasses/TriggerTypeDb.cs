using Domain.Enums;

namespace Infrastructure.DbClasses;

public class TriggerTypeDb
{
    public int Id { get; set; } // [Id] [int] NOT NULL, auto-increment primary key
    public TriggerTypeCode Code { get; set; } // [Code] [char] or [int] depending on enum mapping
    public string Name { get; set; } = string.Empty; // [Description] [varchar](255) NOT NULL
    public int Order { get; set; } // [IsActive] [bit] NOT NULL, default value is true
}