namespace Infrastructure.DbClasses;

//improvement to the Alarms in metronview 2000
public class TriggerDb
{
    public int Id { get; set; } // [Id] [int] NOT NULL, auto-increment primary key
    public int AlarmId { get; set; } // [Code] [varchar](100) NOT NULL
    public int TriggerTypeId { get; set; } // [Name] [varchar](255) NOT NULL
    public int TriggerValue { get; set; } // [Order] [int] NOT NULL, default value is 0
    public int CommunicationModeId { get; set; } // [RecipientModeId] [int] NOT NULL, foreign key to RecipientModeDb
    public string? Subject { get; set; } = string.Empty; // [Subject] [varchar](255) NOT NULL, default value is empty string
    public string? Body { get; set; } = string.Empty; // [Body] [text] NOT NULL, default value is empty string
    public int MinimumSendIntervalMinutes { get; set; } = 0; // [MinimumSendInterval] [int] NOT NULL, default value is 0
    public bool IsEnabled { get; set; } = true; // [IsActive] [bit] NOT NULL, default value is true

    // Navigation property for Alarm
    public AlarmDb Alarm { get; set; } = null!;
    // Navigation property for TriggerType
    public TriggerTypeDb TriggerType { get; set; } = null!;
    // Navigation property for RecipientMode
    public CommunicationModeDb CommunicationMode{ get; set; } = null!;
}
