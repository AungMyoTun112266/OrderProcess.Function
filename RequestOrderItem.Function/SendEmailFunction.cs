using System;
using System.Net.Mail;
using System.Net;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OrderRequest.Function
{
    public class SendEmailFunction
    {
        // All credentials are stored in EnvVariables at Azure function
        private readonly ILogger<SendEmailFunction> _logger;
        private readonly string senderEmail = Environment.GetEnvironmentVariable("SenderEmail");
        private readonly string senderDisplayName = Environment.GetEnvironmentVariable("SenderDisplayName");
        private readonly string senderPassword = Environment.GetEnvironmentVariable("SenderPassword");
        private readonly string recipient = Environment.GetEnvironmentVariable("Recipient");
        private readonly string recipientDisplayName = Environment.GetEnvironmentVariable("RecipientDisplayName");


        public SendEmailFunction(ILogger<SendEmailFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(SendEmailFunction))]
        public async Task Run([QueueTrigger("process-order-queue", Connection = "")] QueueMessage message)
        {
            _logger.LogInformation($"New order received ==> {message.MessageText}");

            await SendEmailAsync(message.MessageText);
        }
        // Send Email
        public async Task SendEmailAsync(string text)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    using (var message = new MailMessage(
                        from: new MailAddress(senderEmail, senderDisplayName),
                        to: new MailAddress(recipient, recipientDisplayName)
                        ))
                    {

                        message.Subject = "Order Request";
                        message.Body = text;

                        await client.SendMailAsync(message);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed email sending ==> {e.Message}");
                throw;
            }
        }
    }
}
