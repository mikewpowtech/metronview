namespace Domain;

public class Sensor
{
    public int Id { get; set; }
    public int UnitId { get; set; }  // Fixed: Initialize with a new Unit object
    public Unit? Unit { get; set; } // Fixed: Initialize with a new Unit object
    public byte Channel { get; set; } // [Channel] [tinyint] NOT NULL
    public byte? ChannelType { get; set; } // [ChannelType] [tinyint] NULL
    public int? LowValue { get; set; } // [LowValue] [int] NULL
    public int? HighValue { get; set; } // [HighValue] [int] NULL
    public string? EngineeringUnits { get; set; } // [EngineeringUnits] [varchar](23) NULL
    public string? Name { get; set; } // [Name] [varchar](100) NULL
    public int? CompanyId { get; set; } // [Code] [varchar](100) NOT NULL
    public int? AlarmId { get; set; } // [Code] [varchar](100) NOT NULL
}