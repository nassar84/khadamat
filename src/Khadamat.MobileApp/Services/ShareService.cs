using Khadamat.Shared.Interfaces;

namespace Khadamat.MobileApp.Services;

public class ShareService : IShareService
{
    public async Task ShareTextAsync(string text, string title = "مشاركة")
    {
        try
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = text,
                Title = title
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Share text error: {ex.Message}");
        }
    }

    public async Task ShareLinkAsync(string url, string title = "مشاركة رابط")
    {
        try
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Uri = url,
                Title = title
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Share link error: {ex.Message}");
        }
    }

    public async Task ShareFileAsync(string filePath, string title = "مشاركة ملف")
    {
        try
        {
            if (!File.Exists(filePath))
            {
                var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page != null)
                    await page.DisplayAlert("خطأ", "الملف غير موجود", "حسناً");
                return;
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = title,
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Share file error: {ex.Message}");
            var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page != null)
                await page.DisplayAlert("خطأ", "تعذرت مشاركة الملف", "حسناً");
        }
    }

    public async Task ShareFilesAsync(List<string> filePaths, string title = "مشاركة ملفات")
    {
        try
        {
            var shareFiles = filePaths
                .Where(File.Exists)
                .Select(path => new ShareFile(path))
                .ToList();

            if (!shareFiles.Any())
            {
                var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page != null)
                    await page.DisplayAlert("خطأ", "لا توجد ملفات للمشاركة", "حسناً");
                return;
            }

            await Share.Default.RequestAsync(new ShareMultipleFilesRequest
            {
                Title = title,
                Files = shareFiles
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Share files error: {ex.Message}");
            var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page != null)
                await page.DisplayAlert("خطأ", "تعذرت مشاركة الملفات", "حسناً");
        }
    }

    public async Task ShareImageWithTextAsync(string imageUrl, string text, string title = "مشاركة خدمة")
    {
        string? tempFile = null;
        try
        {
            // Download the card image to a temp file
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

            tempFile = Path.Combine(FileSystem.CacheDirectory, $"khadamat_share_{Guid.NewGuid():N}.jpg");
            await File.WriteAllBytesAsync(tempFile, imageBytes);

            // Share via native Android share sheet (image + text)
#if ANDROID
            var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? Android.App.Application.Context;
            var file = new Java.IO.File(tempFile);
            var fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(context, $"{context.PackageName}.fileprovider", file);
            
            var intent = new Android.Content.Intent(Android.Content.Intent.ActionSend);
            intent.SetType("image/jpeg");
            intent.PutExtra(Android.Content.Intent.ExtraStream, fileUri);
            intent.PutExtra(Android.Content.Intent.ExtraText, text);
            intent.PutExtra(Android.Content.Intent.ExtraSubject, title);
            intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);

            var chooser = Android.Content.Intent.CreateChooser(intent, title);
            chooser.AddFlags(Android.Content.ActivityFlags.NewTask);
            context.StartActivity(chooser);
#else
            await Share.Default.RequestAsync(new ShareMultipleFilesRequest
            {
                Title = title,
                Files = new List<ShareFile> { new ShareFile(tempFile, "image/jpeg") }
            });
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ShareImageWithText error: {ex.Message}");
            // Fallback: share text only
            try
            {
                await Share.Default.RequestAsync(new ShareTextRequest { Title = title, Text = text });
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"ShareImageWithText fallback error: {fallbackEx.Message}");
            }
        }
        finally
        {
            // Clean up temp file after a short delay
            if (tempFile != null)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(30_000);
                    try { File.Delete(tempFile); } catch { }
                });
            }
        }
    }
}
