using System.Net.Mail;

namespace Presentation.AlarmServer.Email;

public class MailAddressAndFroms
{
    public string From { get; init; }
    public string ReplyTo { get; init; }
    public MailAddress To { get; init; }
}