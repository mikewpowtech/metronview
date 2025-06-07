namespace Domain;

public class Unit
{
    public string Id { get; set; } = string.Empty;
    public string UnitTypeId { get; set; } = string.Empty; // char(4), NOT NULL
    public string? PhoneNumber { get; set; } // char(20), NULL
    public string? PIN { get; set; } // char(4), NULL
    public string ManufacturerID { get; set; } = string.Empty; // char(20), NOT NULL
    public string? ClientID { get; set; } // varchar(100), NULL
    public string? Secret { get; set; } // varchar(100), NULL
    public string UnitStatusID { get; set; } = string.Empty; // char(1), NOT NULL
    public string? CompanyID { get; set; } // Changed from int? to string?
    public int? DaysBeforeNotReported { get; set; } // int, NULL
    public string? CustomFieldValues { get; set; } // nvarchar(max), NULL
}