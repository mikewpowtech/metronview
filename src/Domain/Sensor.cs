namespace Domain;

public class Sensor
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // Changed from int to string
    public Unit Unit { get; set; } = new Unit(); // Fixed: Initialize with a new Unit object
    public byte Channel { get; set; } // [Channel] [tinyint] NOT NULL
    public byte? ChannelType { get; set; } // [ChannelType] [tinyint] NULL
    public int? LowValue { get; set; } // [LowValue] [int] NULL
    public int? HighValue { get; set; } // [HighValue] [int] NULL
    public string? EngineeringUnits { get; set; } // [EngineeringUnits] [varchar](23) NULL
    public string? Name { get; set; } // [Name] [varchar](100) NULL
    public Company? Company { get; set; } // Changed from int? to string?
}