using System;
using System.Net;
using System.Threading.Tasks;
using Khadamat.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Khadamat.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        try
        {
            var server = _configuration["SmtpSettings:Server"] ?? "smtp.gmail.com";
            var portStr = _configuration["SmtpSettings:Port"];
            var port = int.TryParse(portStr, out var p) ? p : 465;
            var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? _configuration["SmtpSettings:Username"] ?? "nassar84@gmail.com";
            var senderName = _configuration["SmtpSettings:SenderName"] ?? "خدماتي - Khadamawi";
            var username = _configuration["SmtpSettings:Username"] ?? senderEmail;
            var password = (_configuration["SmtpSettings:Password"] ?? "").Replace(" ", "").Trim();

            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("SMTP Password is empty in configuration. Email to {ToEmail} skipped.", toEmail);
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 15000;

            // Automatically choose SecureSocketOptions based on port
            var security = port == 465 
                ? SecureSocketOptions.SslOnConnect 
                : (port == 587 ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

            try
            {
                await client.ConnectAsync(server, port, security);
            }
            catch (Exception) when (port != 465)
            {
                // Fallback to SSL on 465 if port 587 was blocked by network/ISP
                _logger.LogWarning("Connecting on port {Port} failed, attempting fallback to port 465 (SSL)...", port);
                await client.ConnectAsync(server, 465, SecureSocketOptions.SslOnConnect);
            }

            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email successfully sent to {ToEmail} with subject: {Subject}", toEmail, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending email to {ToEmail}: {Message}", toEmail, ex.Message);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
    {
        var subject = "إعادة تعيين كلمة المرور - تطبيق وموقع خدماتي";
        var safeUserName = WebUtility.HtmlEncode(userName ?? "عزيزنا العميل");
        var safeLink = WebUtility.HtmlEncode(resetLink);

        var template = @"<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
    <meta charset=""UTF-8"">
    <title>إعادة تعيين كلمة المرور</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f7fc; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; direction: rtl; text-align: right;"">
    <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f4f7fc; padding: 30px 15px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 16px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08); overflow: hidden;"">
                    <tr>
                        <td style=""background: linear-gradient(135deg, #6366f1 0%, #a855f7 100%); padding: 35px 25px; text-align: center; color: #ffffff;"">
                            <h1 style=""margin: 0; font-size: 26px; font-weight: 700;"">خدماتي - Khadamawi</h1>
                            <p style=""margin: 8px 0 0; font-size: 15px; opacity: 0.9;"">منصة الخدمات الأولى</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 35px 30px;"">
                            <h2 style=""color: #1e293b; font-size: 20px; margin-top: 0; margin-bottom: 16px;"">مرحباً {0}،</h2>
                            <p style=""color: #475569; font-size: 15px; line-height: 1.8; margin-bottom: 24px;"">
                                لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك في منصة وتطبيق <strong>خدماتي</strong>.<br />
                                للبدء في تعيين كلمة مرور جديدة، يرجى الضغط على الزر التالي:
                            </p>
                            <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin: 30px 0;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{1}"" target=""_blank"" style=""display: inline-block; background: linear-gradient(135deg, #6366f1 0%, #a855f7 100%); color: #ffffff; text-decoration: none; padding: 14px 36px; border-radius: 30px; font-size: 16px; font-weight: bold; box-shadow: 0 4px 15px rgba(99, 102, 241, 0.4); text-align: center;"">
                                            إعادة تعيين كلمة المرور
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #64748b; font-size: 13px; line-height: 1.6; margin-top: 25px;"">
                                إذا كان الزر أعلاه لا يعمل، يمكنك نسخ الرابط التالي ولصقه في متصفحك مباشرة:
                            </p>
                            <p style=""direction: ltr; text-align: left; background-color: #f1f5f9; padding: 12px; border-radius: 8px; word-break: break-all; font-size: 12px; color: #334155; margin-bottom: 25px;"">
                                <a href=""{1}"" style=""color: #4f46e5; text-decoration: none;"">{1}</a>
                            </p>
                            <div style=""border-top: 1px solid #e2e8f0; padding-top: 20px; margin-top: 25px;"">
                                <p style=""color: #94a3b8; font-size: 13px; line-height: 1.6; margin: 0;"">
                                    🔒 إذا لم تكن قد طلبت إعادة تعيين كلمة المرور، يرجى تجاهل هذه الرسالة بأمان، ولن يتم تغيير أي شيء في حسابك.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8fafc; padding: 20px; text-align: center; border-top: 1px solid #e2e8f0;"">
                            <p style=""color: #94a3b8; font-size: 12px; margin: 0;"">
                                © 2026 تطبيق ومنصة خدماتي. جميع الحقوق محفوظة.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

        var body = string.Format(template, safeUserName, safeLink);
        return await SendEmailAsync(toEmail, subject, body);
    }
}
