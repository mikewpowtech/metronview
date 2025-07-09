using System;

namespace Infrastructure.DbClasses;

public class MostRecentReadingDb
{
    // Primary key: SensorId (unique per sensor)
    public int SensorId { get; set; } // FK to SensorDb.Id, NOT NULL
    public int UnitId { get; set; } // FK to UnitDb.Id, NOT NULL
    public DateTime DateRecordedUtc { get; set; } // NOT NULL
    public DateTime DateReceivedUtc { get; set; } // NOT NULL
    public double? Value { get; set; }

    // Navigation properties
    public SensorDb? Sensor { get; set; }
    public UnitDb? Unit { get; set; }
}
