using System.Threading.Tasks;

namespace Khadamat.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage);
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);
}