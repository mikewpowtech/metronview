namespace Infrastructure.DbClasses;

public class UnitDb
{
    public int Id { get; set; } // int, auto-increment primary key
    public string UnitTypeId { get; set; } = string.Empty; // char(4), NOT NULL
    public string? PhoneNumber { get; set; } // char(20), NULL
    public string? PIN { get; set; } // char(4), NULL
    public string ManufacturerCode { get; set; } = string.Empty; // char(20), NOT NULL
    public string? UnitCode { get; set; } // varchar(100), NULL
    public string? Secret { get; set; } // varchar(100), NULL
    public UnitStatus Status { get; set; }
    public int CompanyID { get; set; }// Now required (non-nullable)
    
    // Foreign key navigation property
    public CompanyDb? Company { get; set; }

    public int? DaysBeforeNotReported { get; set; } // int, NULL
    public string? CustomFieldValues { get; set; } // nvarchar(max), NULL
}