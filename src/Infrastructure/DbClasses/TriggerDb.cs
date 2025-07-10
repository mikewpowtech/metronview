namespace Infrastructure.DbClasses;

//improvement to the Alarms in metronview 2000
using Domain.Enums;

public class TriggerDb
{
    public int Id { get; set; } // [Id] [int] NOT NULL, auto-increment primary key
    public int AlarmId { get; set; } // [AlarmId] [int] NOT NULL
    public TriggerTypeCode TriggerTypeCode { get; set; } // [TriggerTypeCode] [char] or [int] depending on enum mapping
    public int TriggerValue { get; set; } // [Order] [int] NOT NULL, default value is 0
    public int CommunicationModeId { get; set; } // [RecipientModeId] [int] NOT NULL, foreign key to RecipientModeDb
    public string? Subject { get; set; } = string.Empty; // [Subject] [varchar](255) NOT NULL, default value is empty string
    public string? Body { get; set; } = string.Empty; // [Body] [text] NOT NULL, default value is empty string
    public int MinimumSendIntervalMinutes { get; set; } = 0; // [MinimumSendInterval] [int] NOT NULL, default value is 0
    public bool IsEnabled { get; set; } = true; // [IsActive] [bit] NOT NULL, default value is true

    // Navigation property for Alarm
    public AlarmDb? Alarm { get; set; }
    // Navigation property for TriggerType
    public TriggerTypeDb? TriggerType { get; set; }
    // Navigation property for RecipientMode
    public CommunicationModeDb? CommunicationMode { get; set; }
}
