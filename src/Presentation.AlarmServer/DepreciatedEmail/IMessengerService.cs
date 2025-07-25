using Application.Triggers;
using Presentation.AlarmServer.Models;
using System.Net.Mail;

namespace Presentation.AlarmServer.Email;

public interface IMessengerService
{
    public void SendSmsAsync(MailMessage msg, BreachedTriggerDto alarmTrigger);
    public void SendSmtpAsync(MailMessage msg, BreachedTriggerDto alarmTrigger);
    string DefaultFromEmailAddress { get; }
}