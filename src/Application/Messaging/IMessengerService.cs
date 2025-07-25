using Application.Triggers;
using System.Net.Mail;

namespace Application.Messaging
{
    public interface IMessengerService
    {
        void SendSmsAsync(MailMessage msg, BreachedTriggerDto alarmTrigger);
        void SendSmtpAsync(MailMessage msg, BreachedTriggerDto alarm);
    }
}