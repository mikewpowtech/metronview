namespace Domain;

public class Reading
{
    public DateTime DateReceivedUtc { get; set; } // NOT NULL
    public DateTime DateRecordedUtc { get; set; } // NOT NULL
    public int SensorId { get; set; } // NOT NULL
    public Sensor? Sensor { get; set; } = new Sensor(); // Corrected initialization
    public double? Value1 { get; set; } // Latitude property
    public double? Value2 { get; set; } // Longitude property
}