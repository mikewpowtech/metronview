//equivalent to the AlarmSets in metronview 2000
namespace Infrastructure.DbClasses;

public  class AlarmDb
{
    public int Id { get; set; } // [Id] [int] NOT NULL, auto-increment primary key
    public int CompanyId { get; set; } // [Code] [varchar](100) NOT NULL
    public string Name { get; set; } = string.Empty; // [Name] [varchar](255) NOT NULL
    public int RecipientSetId { get; set; } // [Order] [int] NOT NULL, default value is 0
    public bool IsActive { get; set; } = true; // [IsActive] [bit] NOT NULL, default value is true

    // Navigation property for related triggers
    public ICollection<TriggerDb> Triggers { get; set; } = new List<TriggerDb>();

    // Navigation property for Company
    public CompanyDb Company { get; set; } = null!;

    // Navigation property for RecipientSet
    public RecipientSetDb RecipientSet { get; set; } = null!;
}
