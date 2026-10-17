using Khadamat.Shared.Interfaces;
using Khadamat.MobileApp.Helpers;
using Khadamat.MobileApp.Views.Popups;

namespace Khadamat.MobileApp.Services;

public class CameraService : IDeviceCameraService
{
    public async Task<byte[]?> CapturePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await KhadamatDialogHelper.ShowErrorAsync("تنبيه", "الكاميرا غير متوفرة على هذا الجهاز");
                return null;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null) return null;

            return await LoadPhotoAsync(photo);
        }
        catch (PermissionException)
        {
            await KhadamatDialogHelper.ShowWarningAsync("تنبيه", "يرجى السماح بالوصول إلى الكاميرا لالتقاط الصور");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Camera error: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]?> PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "اختر صورة"
            });

            if (photo == null) return null;

            return await LoadPhotoAsync(photo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Photo picker error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<byte[]>> PickMultiplePhotosAsync(int maxCount = 5)
    {
        var results = new List<byte[]>();
        
        try
        {
            // MAUI doesn't support multiple photo selection natively yet
            // Workaround: Allow user to pick photos one by one
            for (int i = 0; i < maxCount; i++)
            {
                bool shouldContinue = false;
                if (results.Count > 0)
                {
                    shouldContinue = await KhadamatDialogHelper.ShowConfirmAsync(
                        "اختيار صور متعددة",
                        $"تم اختيار {results.Count} صورة بنجاح. هل تريد إضافة المزيد؟",
                        "نعم، إضافة صورة",
                        "اكتفيت بالصور الحالية",
                        MobileDialogType.Info
                    );
                }
                else
                {
                    shouldContinue = true;
                }

                if (!shouldContinue && results.Count > 0) break;

                var photo = await PickPhotoAsync();
                if (photo != null)
                    results.Add(photo);
                else
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Multiple photo picker error: {ex.Message}");
        }

        return results;
    }

    public async Task<byte[]> CompressImageAsync(byte[] imageData, int quality = 80)
    {
        // Basic implementation - for production, use ImageSharp or SkiaSharp
        await Task.CompletedTask;
        return imageData;
    }

    public async Task<bool> IsCameraAvailableAsync()
    {
        await Task.CompletedTask;
        return MediaPicker.Default.IsCaptureSupported;
    }

    private async Task<byte[]> LoadPhotoAsync(FileResult photo)
    {
        using var stream = await photo.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}
