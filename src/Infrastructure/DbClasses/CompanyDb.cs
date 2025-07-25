namespace Infrastructure.DbClasses;

public class CompanyDb
{
    public int Id { get; set; } // Maps to CompanyID (IDENTITY)
    public string Name { get; set; } = string.Empty; // Maps to CompanyName (nvarchar(256), NOT NULL)
    public int? ParentCompanyId { get; set; } // Maps to ManagingCompanyID (int, NULL)
    public string? HostHeader { get; set; } // Maps to HostHeader (nvarchar(255), NULL)
    public int? DefaultDaysHistory { get; set; } // Maps to DefaultDaysHistory (int, NULL)
    public int? DefaultDaysBeforeNotReported { get; set; } // Maps to DefaultDaysBeforeNotReported (int, NULL)
    public string? AlarmEmailFromAddress { get; set; } // Maps to AlarmEmailFromAddress (varchar(254), NULL)
    public string? AlarmEmailReplyToAddress { get; set; } // Maps to AlarmEmailReplyToAddress (varchar(254), NULL)
    public string? AlarmSmsToAddressTemplate { get; set; } // Maps to AlarmSmsToAddressTemplate (varchar(254), NULL)
    public string? AlarmSmsSubjectTemplate { get; set; } // Maps to AlarmSmsSubjectTemplate (nvarchar(max), NULL)
    public string? AlarmSmsBodyTemplate { get; set; } // Maps to AlarmSmsBodyTemplate (nvarchar(max), NULL)
    public int? DaysBeforeRTUDataDeletion { get; set; } // Maps to DaysBeforeRTUDataDeletion (int, NULL)
    public string? CustomFieldDefinitions { get; set; } // Maps to CustomFieldDefinitions (nvarchar(max), NULL)
    public string? Dashboard { get; set; } // Maps to Dashboard (nvarchar(max), NULL)

    // Navigation property for self-referencing relationship
    public CompanyDb? ParentCompany { get; set; }
    public ICollection<CompanyDb> ManagedCompanies { get; set; } = new List<CompanyDb>();
}