namespace Infrastructure.DbClasses;

//improvement to recipeient modes in metronview 2000
public class CommunicationModeDb
{
    public int Id { get; set; } // [Id] [int] NOT NULL, auto-increment primary key
    public string Code { get; set; } = string.Empty; // [Name] [varchar](100) NOT NULL
    public string Name { get; set; } = string.Empty; // [Description] [varchar](255) NOT NULL
    public int Order { get; set; } // [IsActive] [bit] NOT NULL, default value is true
}
