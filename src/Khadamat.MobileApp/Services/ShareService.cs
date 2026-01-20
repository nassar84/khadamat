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
}
