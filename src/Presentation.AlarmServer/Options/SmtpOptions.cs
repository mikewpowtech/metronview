using System.ComponentModel.DataAnnotations;

namespace Presentation.AlarmServer.Options;
public class SmtpOptions
{
    public int Port { get; set; } = 25;

    [Required]
    [RegularExpression(
        @"^(?=.{1,255}$)[0-9A-Za-z](?:(?:[0-9A-Za-z]|-){0,61}[0-9A-Za-z])?(?:\.[0-9A-Za-z](?:(?:[0-9A-Za-z]|-){0,61}[0-9A-Za-z])?)*\.?$",
        ErrorMessage = "{0} Should look like a host name")]
    public string Host { get; set; }

    [EmailAddress] public string DefaultAlarmEmailFromAddress { get; set; } = "alarms@metronview.com";
}