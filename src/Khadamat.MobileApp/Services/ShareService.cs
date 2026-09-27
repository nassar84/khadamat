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
                Text = $"{title}\n{url}",
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
            byte[] imageBytes;

            // Handle base64 data URLs
            if (imageUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                var base64Data = imageUrl.Substring(imageUrl.IndexOf(',') + 1);
                imageBytes = Convert.FromBase64String(base64Data);
            }
            else
            {
                // Download the card image to a temp file
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
                imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            }

            var extension = imageUrl.Contains(".png", StringComparison.OrdinalIgnoreCase) ? "png" : "jpg";
            var mimeType = extension == "png" ? "image/png" : "image/jpeg";

            tempFile = Path.Combine(FileSystem.CacheDirectory, $"khadamat_share_{Guid.NewGuid():N}.{extension}");
            await File.WriteAllBytesAsync(tempFile, imageBytes);

            // Also copy text to clipboard as safety net for apps like Facebook that strip EXTRA_TEXT
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.SetTextAsync(text);
                }
            }
            catch { }

            // Share via native Android share sheet (image + text)
#if ANDROID
            var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? Android.App.Application.Context;
            var file = new Java.IO.File(tempFile);
            
            Android.Net.Uri? fileUri = null;
            try
            {
                // MAUI Essentials FileProvider authority uses capital P: .fileProvider
                fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(context, $"{context.PackageName}.fileProvider", file);
            }
            catch
            {
                try
                {
                    fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(context, $"{context.PackageName}.fileprovider", file);
                }
                catch (Exception fpEx)
                {
                    Console.WriteLine($"FileProvider error: {fpEx.Message}");
                }
            }

            if (fileUri != null)
            {
                var intent = new Android.Content.Intent(Android.Content.Intent.ActionSend);
                intent.SetType(mimeType);
                intent.PutExtra(Android.Content.Intent.ExtraStream, fileUri);
                intent.PutExtra(Android.Content.Intent.ExtraText, text);
                intent.PutExtra(Android.Content.Intent.ExtraSubject, title);
                intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);

                var chooser = Android.Content.Intent.CreateChooser(intent, title);
                chooser.AddFlags(Android.Content.ActivityFlags.NewTask);
                context.StartActivity(chooser);
                return;
            }
#else
            await Share.Default.RequestAsync(new ShareMultipleFilesRequest
            {
                Title = title,
                Files = new List<ShareFile> { new ShareFile(tempFile, mimeType) }
            });
            return;
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
                    await Task.Delay(45_000);
                    try { File.Delete(tempFile); } catch { }
                });
            }
        }
    }
}
