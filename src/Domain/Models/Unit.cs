using Domain.Enums;

namespace Domain;

public class Unit
{
    public int Id { get; set; }
    public int UnitTypeId { get; set; } // char(4), NOT NULL
    public string? PhoneNumber { get; set; } // char(20), NULL
    public string? PIN { get; set; } // char(4), NULL
    public string ManufacturerCode { get; set; } = string.Empty; // char(20), NOT NULL
    public string? ClientCode { get; set; } // varchar(100), NULL
    public string? Secret { get; set; } // varchar(100), NULL
    public UnitStatusType Status { get; set; } // char(1), NOT NULL
    public int? CompanyId { get; set; } // Now required (non-nullable)
    public Company? Company { get; set; }
    public int? DaysBeforeNotReported { get; set; } // int, NULL
    public string? CustomFieldValues { get; set; } // nvarchar(max), NULL
}