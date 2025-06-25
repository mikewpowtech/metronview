namespace Domain;

public class ConfigurationUpload
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string? FileName { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public string? UploadedBy { get; set; }
    public byte[]? Content { get; set; }
    public Unit? Unit { get; set; }
    // Add more properties as needed
}