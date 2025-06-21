namespace Infrastructure.DbClasses;

public class SensorDb
{
    public int Id { get; set; }

    // Foreign key to UnitDb
    public int UnitId { get; set; } // Changed from string to int
    public UnitDb? Unit { get; set; }

    public byte Channel { get; set; }
    public byte? ChannelType { get; set; }
    public int? LowValue { get; set; }
    public int? HighValue { get; set; }
    public string? EngineeringUnits { get; set; }
    public string? Name { get; set; }
}