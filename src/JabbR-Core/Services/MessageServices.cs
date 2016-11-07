using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using Microsoft.Extensions.Options;

namespace JabbR_Core.Services
{
    public class MessageServices
    {
        public class AuthMessageSender : IEmailSender
        {
            public AuthMessageSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
            {
                Options = optionsAccessor.Value;
            }

            public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

            public Task SendEmailAsync(string email, string subject, string message)
            {
                // Plug in your email service here to send an email.
                var myMessage = new SendGridMessage();
                myMessage.AddTo(email);
                myMessage.From = new System.Net.Mail.MailAddress("azure_51e01f459fd9c7591f2bc9de5fd73cf2@azure.com", "JabbRCore");
                myMessage.Subject = subject;
                myMessage.Text = message;
                myMessage.Html = message;
                var credentials = new System.Net.NetworkCredential(
                    Options.SendGridUser,
                    Options.SendGridKey);
                // Create a Web transport for sending email.
                var transportWeb = new SendGrid.Web(credentials);
                return transportWeb.DeliverAsync(myMessage);
            }
            
        }
    }
}
