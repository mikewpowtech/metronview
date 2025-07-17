using Domain.Enums;
namespace Domain;

public class Trigger
{
    public int Id { get; set; } // [Id] [int] NOT NULL, auto-increment primary key
    public int AlarmId { get; set; } // [AlarmId] [int] NOT NULL
    public int TriggerTypeId { get; set; } // [TriggerTypeId] [int] NOT NULL, foreign key to TriggerType
    public int TriggerValue { get; set; } // [Order] [int] NOT NULL, default value is 0
    public int CommunicationModeId { get; set; } // [RecipientModeId] [int] NOT NULL, foreign key to RecipientModeDb
    public string? Subject { get; set; } = string.Empty; // [Subject] [varchar](255) NOT NULL, default value is empty string
    public string? Body { get; set; } = string.Empty; // [Body] [text] NOT NULL, default value is empty string
    public int MinimumSendIntervalMinutes { get; set; } = 0; // [MinimumSendInterval] [int] NOT NULL, default value is 0
    public bool IsEnabled { get; set; } = true; // [IsActive] [bit] NOT NULL, default value is true

    // Navigation property for Alarm
    public Alarm? Alarm { get; set; }
    // Navigation property for TriggerType
    public TriggerType? TriggerType { get; set; }
    // Navigation property for RecipientMode
    public CommunicationMode? CommunicationMode { get; set; }
}
