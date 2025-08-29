using Application.Options;
using Application.Triggers;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace Application.Messaging;

public class AzureEmailService : IMessengerService
{
    private readonly EmailClient _emailClient;
    private readonly AzureEmailOptions _options;
    private readonly ILogger<AzureEmailService> _logger;

    public AzureEmailService(IOptions<AzureEmailOptions> options, ILogger<AzureEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _emailClient = new EmailClient(_options.ConnectionString);
    }

    public async void SendSmsAsync(MailMessage msg, BreachedTriggerDto alarmTrigger)
    {
        // For SMS, you could use Azure Communication Services SMS instead
        // or keep this as email for now
        await SendEmailAsync(msg, alarmTrigger);
    }

    public async void SendSmtpAsync(MailMessage msg, BreachedTriggerDto alarmTrigger)
    {
        await SendEmailAsync(msg, alarmTrigger);
    }

    private async Task SendEmailAsync(MailMessage mailMessage, BreachedTriggerDto alarmTrigger)
    {
        try
        {
            var emailMessage = new EmailMessage(
                senderAddress: _options.DefaultFromAddress,
                content: new EmailContent(mailMessage.Subject)
                {
                    PlainText = mailMessage.IsBodyHtml ? null : mailMessage.Body,
                    Html = mailMessage.IsBodyHtml ? mailMessage.Body : null
                },
                recipients: new EmailRecipients(
                    to: mailMessage.To.Select(addr => new EmailAddress(addr.Address, addr.DisplayName)).ToList()
                )
            );

            // Add CC recipients if any
            //if (mailMessage.CC.Count > 0)
            //{
                // Replace the following lines:
                // emailMessage.Recipients.CC.AddRange(
                //     mailMessage.CC.Select(addr => new EmailAddress(addr.Address, addr.DisplayName))
                // );

                // emailMessage.Recipients.BCC.AddRange(
                //     mailMessage.Bcc.Select(addr => new EmailAddress(addr.Address, addr.DisplayName))
                // );

                // With the following code:
                foreach (var addr in mailMessage.CC)
                {
                    emailMessage.Recipients.CC.Add(new EmailAddress(addr.Address, addr.DisplayName));
                }

                foreach (var addr in mailMessage.Bcc)
                {
                    emailMessage.Recipients.BCC.Add(new EmailAddress(addr.Address, addr.DisplayName));
                }
            // Replace this line:
            // var response = await _emailClient.SendAsync(emailMessage);

            // With the following line:
            var response = await _emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);

            if (response.HasValue)
            {
                _logger.LogInformation("Email sent successfully for {TriggerType} alarm. Recipients: {Recipients}, Status: {Status}, Sensor ID: {SensorId}", 
                    alarmTrigger.TriggerType?.Name ?? "Unknown",
                    string.Join(", ", mailMessage.To.Select(t => t.Address)), 
                    response.Value.Status,
                    alarmTrigger.SensorId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Azure Communication Services to {Recipients}", 
                string.Join(", ", mailMessage.To.Select(t => t.Address)));
        }
    }
}