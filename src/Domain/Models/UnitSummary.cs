namespace Domain;

public class UnitSummary
{
    public int Id { get; set; }
    public string? UnitType { get; set; }
    public DateTime? LastComms { get; set; }
    public bool? Alarm { get; set; }
    public List<Reading> Readings { get; set; } = new List<Reading>();
    public List<Sensor> Sensors { get; set; } = new List<Sensor>();
    public double? AmbientTemperature { get; set; }
    public string? Carrier { get; set; }
    public int? Signal { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? UnitCode { get; set; }
    public int? CompanyID { get; set; }
    public string? Company { get; set; }
}