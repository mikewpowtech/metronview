namespace Domain;

public class ConfigurationUpload
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string? Configuration { get; set; }
    public DateTime DateCreatedUtc { get; set; }
    public string? QueueingUserName { get; set; }
    public byte[]? Content { get; set; }
    public Unit? Unit { get; set; }
    // Add more properties as needed
}