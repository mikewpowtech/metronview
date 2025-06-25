namespace Infrastructure.DbClasses;

public class UnitStatusDb
{
    // Foreign key to UnitDb
    public DateTime DateReceivedUtc { get; set; } // NOT NULL
    public int UnitId { get; set; } // Changed from string to int
    public UnitDb? Unit { get; set; }

    public bool Mip { get; set; }
    public bool FailedCallout { get; set; }
    public bool BatteryAlarm { get; set; }
    public bool AutoConfig { get; set; }
    public float? Temperature { get; set; }
    public string? Carrier { get; set; }
    public float? Signal { get; set; }
}