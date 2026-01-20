using Khadamat.Shared.Interfaces;

namespace Khadamat.MobileApp.Services;

public class PhoneService : IPhoneService
{
    public bool CanMakePhoneCalls => PhoneDialer.Default.IsSupported;

    public async Task MakePhoneCallAsync(string phoneNumber)
    {
        try
        {
            if (!CanMakePhoneCalls)
            {
                var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page != null)
                    await page.DisplayAlert("خطأ", "المكالمات الهاتفية غير مدعومة على هذا الجهاز", "حسناً");
                return;
            }

            // Clean phone number
            phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(phoneNumber))
            {
                var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page != null)
                    await page.DisplayAlert("خطأ", "رقم الهاتف غير صحيح", "حسناً");
                return;
            }

            PhoneDialer.Default.Open(phoneNumber);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Phone call error: {ex.Message}");
            var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page != null)
                await page.DisplayAlert("خطأ", "تعذر إجراء المكالمة", "حسناً");
        }
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            if (!Sms.Default.IsComposeSupported)
            {
                var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page != null)
                    await page.DisplayAlert("خطأ", "الرسائل النصية غير مدعومة على هذا الجهاز", "حسناً");
                return;
            }

            var smsMessage = new SmsMessage(message, new[] { phoneNumber });
            await Sms.Default.ComposeAsync(smsMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SMS error: {ex.Message}");
            var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page != null)
                await page.DisplayAlert("خطأ", "تعذر إرسال الرسالة", "حسناً");
        }
    }

    public async Task OpenWhatsAppChatAsync(string phoneNumber, string? message = null)
    {
        try
        {
            // Clean phone number and ensure it starts with country code
            phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // If Egyptian number starting with 0, replace with +20
            if (phoneNumber.StartsWith("0"))
            {
                phoneNumber = "20" + phoneNumber.Substring(1);
            }
            else if (!phoneNumber.StartsWith("20"))
            {
                phoneNumber = "20" + phoneNumber;
            }

            var whatsappUrl = $"https://wa.me/{phoneNumber}";
            
            if (!string.IsNullOrEmpty(message))
            {
                whatsappUrl += $"?text={Uri.EscapeDataString(message)}";
            }

            var uri = new Uri(whatsappUrl);
            var canOpen = await Launcher.Default.CanOpenAsync(uri);

            if (canOpen)
            {
                await Launcher.Default.OpenAsync(uri);
            }
            else
            {
                // Try opening WhatsApp app directly
                var appUri = new Uri($"whatsapp://send?phone={phoneNumber}");
                await Launcher.Default.OpenAsync(appUri);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WhatsApp error: {ex.Message}");
            
            bool installWhatsApp = false;
            var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page != null)
            {
                installWhatsApp = await page.DisplayAlert(
                    "واتساب غير مثبت",
                    "يبدو أن واتساب غير مثبت على جهازك. هل تريد تثبيته؟",
                    "نعم",
                    "لا"
                );
            }

            if (installWhatsApp)
            {
                var storeUrl = DeviceInfo.Platform == DevicePlatform.Android
                    ? "https://play.google.com/store/apps/details?id=com.whatsapp"
                    : "https://apps.apple.com/app/whatsapp-messenger/id310633997";

                await Launcher.Default.OpenAsync(new Uri(storeUrl));
            }
        }
    }

    public async Task<bool> IsWhatsAppInstalledAsync()
    {
        try
        {
            var uri = new Uri("whatsapp://");
            return await Launcher.Default.CanOpenAsync(uri);
        }
        catch
        {
            return false;
        }
    }
}
