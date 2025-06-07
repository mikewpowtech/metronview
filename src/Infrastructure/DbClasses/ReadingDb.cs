namespace Infrastructure.DbClasses;

public class ReadingDb
{
    // Compound primary key: DateRecordedUtc + SensorId
    public DateTime DateRecordedUtc { get; set; } // NOT NULL
    public int SensorId { get; set; } // Changed from string to int, FK to SensorDb.Id, NOT NULL

    public DateTime DateReceivedUtc { get; set; } // NOT NULL

    public double? Value1 { get; set; }
    public double? Value2 { get; set; }

    // Navigation property
    public SensorDb? Sensor { get; set; }
}