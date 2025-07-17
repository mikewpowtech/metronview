namespace Infrastructure.DbClasses;

//improvement to the Alarms in metronview 2000
using Domain.Enums;

public class TriggerDb
{
    public int Id { get; set; } // [Id] [int] NOT NULL, auto-increment primary key
    public int AlarmId { get; set; } // [AlarmId] [int] NOT NULL
    public int TriggerTypeId { get; set; } // [TriggerTypeId] [int] NOT NULL, foreign key to TriggerTypeDb
    public int TriggerValue { get; set; } // [TriggerValue] [int] NOT NULL, default value is 0
    public int CommunicationModeId { get; set; } // [CommunicationModeId] [int] NOT NULL, foreign key to CommunicationModeDb
    public string? Subject { get; set; } = string.Empty; // [Subject] [varchar](255) NOT NULL, default value is empty string
    public string? Body { get; set; } = string.Empty; // [Body] [text] NOT NULL, default value is empty string
    public int MinimumSendIntervalMinutes { get; set; } = 0; // [MinimumSendInterval] [int] NOT NULL, default value is 0
    public bool IsEnabled { get; set; } = true; // [IsActive] [bit] NOT NULL, default value is true

    // Navigation property for Alarm
    public AlarmDb? Alarm { get; set; }
    
    // Navigation property for TriggerType - REQUIRED (non-nullable)
    public TriggerTypeDb TriggerType { get; set; } = null!;
    
    // Navigation property for CommunicationMode - REQUIRED (non-nullable)
    public CommunicationModeDb CommunicationMode { get; set; } = null!;
}
