using System.Threading.Tasks;

namespace Khadamat.Shared.Interfaces;

/// <summary>
/// Interface for phone integration (calls, SMS, WhatsApp)
/// </summary>
public interface IPhoneService
{
    /// <summary>
    /// Make a phone call
    /// </summary>
    Task MakePhoneCallAsync(string phoneNumber);

    /// <summary>
    /// Send SMS
    /// </summary>
    Task SendSmsAsync(string phoneNumber, string message);

    /// <summary>
    /// Open WhatsApp chat
    /// </summary>
    Task OpenWhatsAppChatAsync(string phoneNumber, string? message = null);

    /// <summary>
    /// Check if phone can make calls
    /// </summary>
    bool CanMakePhoneCalls { get; }

    /// <summary>
    /// Check if WhatsApp is installed
    /// </summary>
    Task<bool> IsWhatsAppInstalledAsync();
}
