using Presentation.AlarmServer.Models;
using System.Net.Mail;

namespace Presentation.AlarmServer.Email;

public interface IMessengerService
{
    public void SendSmsAsync(MailMessage msg, AlarmServerDto alarmTrigger);
    public void SendSmtpAsync(MailMessage msg, AlarmServerDto alarmTrigger);
    string DefaultFromEmailAddress { get; }
}