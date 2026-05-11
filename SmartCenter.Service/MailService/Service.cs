using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace SmartCenter.Service.MailService;

public class Service : IService
{
    private readonly MailOptions _mailOptions = new();

    public Service(IConfiguration configuration)
    {
        configuration.GetSection(nameof(MailOptions)).Bind(_mailOptions);
    }
    
    public async Task SendMail(MailContent mailContent)
    {
        var email = new MimeMessage();
        email.Sender = new MailboxAddress(_mailOptions?.DisplayName, _mailOptions!.Mail);
        email.From.Add(new MailboxAddress(_mailOptions?.DisplayName, _mailOptions!.Mail));
        email.To.Add(MailboxAddress.Parse(mailContent.To));
        email.Subject = mailContent.Subject;

        var builder = new BodyBuilder { HtmlBody = mailContent.Body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        smtp.Timeout = 60000;

        await smtp.ConnectAsync(_mailOptions!.Host, _mailOptions.Port,
            SecureSocketOptions.SslOnConnect); 

        await smtp.AuthenticateAsync("resend", _mailOptions.Password);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}