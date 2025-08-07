using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PapeleriaApi.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendVerificationEmailAsync(string toEmail, string verificationLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpConfig = _configuration.GetSection("Smtp");
                
                var portString = smtpConfig["Port"];
                if (string.IsNullOrWhiteSpace(portString))
                {
                    throw new InvalidOperationException("SMTP port configuration is missing.");
                }

                using var client = new SmtpClient(smtpConfig["Host"], int.Parse(portString))
                {
                    EnableSsl = bool.Parse(smtpConfig["EnableSsl"] ?? "true"),
                    Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"])
                };

                var fromEmail = smtpConfig["Username"];
                if (string.IsNullOrWhiteSpace(fromEmail))
                {
                    throw new InvalidOperationException("SMTP username (from email) configuration is missing.");
                }

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Papelería Web"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(toEmail);
                
                await client.SendMailAsync(mailMessage);
                
                _logger.LogInformation("Email sent successfully to: {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to: {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendVerificationEmailAsync(string toEmail, string verificationLink)
        {
            var subject = "Verifica tu correo - Papelería Web";
            var body = $@"Hola,

Gracias por registrarte en Papelería Web. Para completar tu registro y verificar tu correo electrónico, por favor haz clic en el siguiente enlace:

{verificationLink}

Este enlace expirará en 24 horas por seguridad.

Si no has solicitado este registro, puedes ignorar este correo.

Saludos,
El equipo de Papelería Web";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}
