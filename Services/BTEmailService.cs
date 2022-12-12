using BugTrackerMVC.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BugTrackerMVC.Services
{
    public class BTEmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        public BTEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSender = _mailSettings.EmailAddress ?? Environment.GetEnvironmentVariable("EmailAddress");
            MimeMessage newEmail = new();
            newEmail.Sender = MailboxAddress.Parse(emailSender);

            foreach (string emailAddress in email.Split(";"))
            {
                newEmail.To.Add(MailboxAddress.Parse(emailAddress));
            }

            newEmail.Subject = subject;
            BodyBuilder emailBody = new();
            emailBody.HtmlBody = htmlMessage;
            newEmail.Body = emailBody.ToMessageBody();
            //AT this point lets log into out smtp client
            using SmtpClient smtpClient = new();

            try
            {
                string? emailHost = _mailSettings.EmailHost ?? Environment.GetEnvironmentVariable("EmailHost");
                int emailPort = _mailSettings.EmailPort != 0 ? _mailSettings.EmailPort : int.Parse(Environment.GetEnvironmentVariable("EmailPort")!);
                string? emailPassword = _mailSettings.EmailPassword ?? Environment.GetEnvironmentVariable("EmailPassword");
                await smtpClient.ConnectAsync(emailHost, emailPort, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailSender, emailPassword);
                await smtpClient.SendAsync(newEmail);
                await smtpClient.DisconnectAsync(true);

            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }
    }
}
