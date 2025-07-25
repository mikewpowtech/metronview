namespace Domain;

public class UnitStatus
{
    public int UnitId { get; set; }
    public DateTime DateReceivedUtc { get; set; }
    
    public bool Mip { get; set; }
    public bool FailedCallout { get; set; }
    public bool BattAlarm { get; set; }
    public bool AutoConfig { get; set; }
    public float? Temperature { get; set; }
    public string? Carrier { get; set; }
    public float? Signal { get; set; }
    
    // Navigation property to Unit domain entity
    public Unit? Unit { get; set; }
}