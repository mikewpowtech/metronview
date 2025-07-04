namespace Presentation.Api.Models;

public class AlarmDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RecipientSetId { get; set; }
    public bool IsActive { get; set; } = true;
}
