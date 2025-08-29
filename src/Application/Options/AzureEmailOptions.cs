using System.ComponentModel.DataAnnotations;

namespace Application.Options;

public class AzureEmailOptions
{
    [Required]
    public string ConnectionString { get; set; }
    
    [Required]
    [EmailAddress]
    public string DefaultFromAddress { get; set; }
    
    public string DefaultFromDisplayName { get; set; } = "Alarm System";
}