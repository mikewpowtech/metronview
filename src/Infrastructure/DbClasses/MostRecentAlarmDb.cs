namespace Infrastructure.DbClasses;

public class MostRecentAlarmDb
{
    public int SensorId { get; set; }
    public int AlarmId { get; set; }
    public DateTime MostRecentSendUtc { get; set; }
    
    // Navigation properties if needed
    public SensorDb? Sensor { get; set; }
    public AlarmDb? Alarm { get; set; }
}