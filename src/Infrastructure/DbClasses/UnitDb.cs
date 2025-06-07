namespace Infrastructure.DbClasses;

public class UnitDb
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UnitTypeId { get; set; } = string.Empty; // char(4), NOT NULL
    public string? PhoneNumber { get; set; } // char(20), NULL
    public string? PIN { get; set; } // char(4), NULL
    public string ManufacturerCode { get; set; } = string.Empty; // char(20), NOT NULL
    public string? UnitCode { get; set; } // varchar(100), NULL
    public string? Secret { get; set; } // varchar(100), NULL
    public string UnitStatusID { get; set; } = string.Empty; // char(1), NOT NULL
    public string? CompanyID { get; set; } // string, NULL

    // Foreign key navigation property
    public CompanyDb? Company { get; set; }

    public int? DaysBeforeNotReported { get; set; } // int, NULL
    public string? CustomFieldValues { get; set; } // nvarchar(max), NULL
}