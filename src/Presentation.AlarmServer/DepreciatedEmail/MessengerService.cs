using Application.Triggers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Options;
using System;
using System.Net.Mail;

namespace Presentation.AlarmServer.Email;

public class MessengerService:IMessengerService
{
    private readonly SmtpOptions options;
    private readonly ILogger<MessengerService> logger;
    public string DefaultFromEmailAddress { get; init; }

    public MessengerService(IOptions<SmtpOptions> options, ILogger<MessengerService> logger)
    {
        this.options = options.Value;
        this.logger = logger;
        DefaultFromEmailAddress = this.options.DefaultAlarmEmailFromAddress;
    }

    public void SendSmsAsync(MailMessage msg, BreachedTriggerDto alarmTrigger)
    {
        // Note that there's no need to retain an explicit reference to the client;
        // it'll be referenced by a stack frame somewhere until the async call completes, at which point we trash it.

        SendMessageAsync(msg, alarmTrigger);
    }
    public void SendSmtpAsync(MailMessage msg, BreachedTriggerDto alarmTrigger)
    {
        SendMessageAsync(msg, alarmTrigger);
    }

    private void SendMessageAsync(MailMessage msg, BreachedTriggerDto alarm)
    {
        var smtpClient = new SmtpClient(options.Host, options.Port);
        smtpClient.SendCompleted += (_, args) =>
        {
            logger.LogInformation("Message sent to {To}", msg.To);
            if (args.Cancelled)
            {
                logger.LogWarning("Could not send message to {To} because it was cancelled", msg.To);
            }
            else if (args.Error != null)
            {
                logger.LogError(args.Error, "Could not send message to {To} because an error occurred", msg.To);
            }
            (args.UserState as IDisposable)?.Dispose();
        };

        smtpClient.SendAsync(msg, smtpClient);
    }

    //void IMessengerService.SendSmsAsync(MailMessage msg, AlarmTriggerDto alarmTrigger)
    //{
    //    SendSmsAsync(msg, alarmTrigger);
    //}

    //void IMessengerService.SendSmtpAsync(MailMessage msg, AlarmTriggerDto alarmTrigger)
    //{
    //    SendSmtpAsync(msg, alarmTrigger);
    //}
}