using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Suzubun.Service.Interfaces;
using Suzubun.Service.Models;

namespace Suzubun.Service.Services;

public class EmailService : IEmailService
{
    private readonly SmtpOptions _smtpOptions;

    public EmailService(IOptions<SmtpOptions> smtpOptions)
    {
        _smtpOptions = smtpOptions.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = body
        };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_smtpOptions.FromEmail, _smtpOptions.AppPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
