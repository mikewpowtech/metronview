namespace Infrastructure.DbClasses;

public class SensorDb
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // Foreign key to UnitDb
    public string UnitId { get; set; } = string.Empty;
    public UnitDb? Unit { get; set; }

    public byte Channel { get; set; }
    public byte? ChannelType { get; set; }
    public int? LowValue { get; set; }
    public int? HighValue { get; set; }
    public string? EngineeringUnits { get; set; }
    public string? Name { get; set; }

    // Foreign key to CompanyDb
    public string? CompanyID { get; set; }
    public CompanyDb? Company { get; set; }
}