namespace Infrastructure.DbClasses;

public class RecipientSetDb
{
    public int Id { get; set; } // [Id] [int] NOT NULL, auto-increment primary key
    public int CompanyId { get; set; } // [CompanyId] [int] NOT NULL, FK to CompanyDb.Id
    public string Name { get; set; } = string.Empty; // [Name] [varchar](255) NOT NULL

    // Navigation property for many-to-many
    public ICollection<RecipientDb> Recipients { get; set; } = new List<RecipientDb>();
}
