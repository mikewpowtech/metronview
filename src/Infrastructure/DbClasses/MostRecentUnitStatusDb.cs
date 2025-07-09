using System;

namespace Infrastructure.DbClasses;

public class MostRecentUnitStatusDb
{
    // Primary key: UnitId (unique per unit)
    public int UnitId { get; set; } // FK to UnitDb.Id, NOT NULL
    public DateTime DateReceivedUtc { get; set; } // NOT NULL
    public UnitDb? Unit { get; set; }
    public bool Mip { get; set; }
    public bool FailedCallout { get; set; }
    public bool BatteryAlarm { get; set; }
    public bool AutoConfig { get; set; }
    public float? Temperature { get; set; }
    public string? Carrier { get; set; }
    public float? Signal { get; set; }
}
