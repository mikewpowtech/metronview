namespace Domain;

public class Reading
{
    public DateTime DateReceivedUtc { get; set; } // NOT NULL
    public DateTime DateRecordedUtc { get; set; } // NOT NULL
    public int SensorId { get; set; } // NOT NULL
    public Sensor? Sensor { get; set; } // Corrected initialization
    public int UnitId { get; set; } // NOT NULL
    public Unit? Unit { get; set; } // Corrected initialization
    public double? Value { get; set; } 
}