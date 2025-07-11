using Domain.Enums;

namespace Infrastructure.DbClasses;

public class ConfigurationUploadDb
{
    public int Id { get; set; } // int, auto-increment primary key
    public int UnitId { get; set; } // char(4), NOT NULL
    public DateTime DateCreatedUtc { get; set; }
    public DateTime DateUploadedUtc { get; set; }
    public string? Configuration { get; set; } // char(4), NULL
    public string? QueueingUserName { get; set; } // char(4), NULL
    public ConfigurationUploadStatus UploadStatusId { get; set; }

    // Foreign key navigation property
    public UnitDb? Unit { get; set; }
}
