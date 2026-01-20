using Khadamat.Shared.Interfaces;

namespace Khadamat.MobileApp.Services;

public class FilePickerService : IFilePickerService
{
    public async Task<FilePickerResult?> PickFileAsync(string[] allowedTypes)
    {
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, allowedTypes },
                { DevicePlatform.Android, allowedTypes },
                { DevicePlatform.WinUI, allowedTypes }
            });

            var options = new PickOptions
            {
                PickerTitle = "اختر ملف",
                FileTypes = customFileType
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result == null) return null;

            return await ConvertToFilePickerResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File picker error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<FilePickerResult>> PickMultipleFilesAsync(string[] allowedTypes, int maxCount = 10)
    {
        var results = new List<FilePickerResult>();

        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, allowedTypes },
                { DevicePlatform.Android, allowedTypes },
                { DevicePlatform.WinUI, allowedTypes }
            });

            var options = new PickOptions
            {
                PickerTitle = "اختر ملفات",
                FileTypes = customFileType
            };

            var pickedFiles = await FilePicker.Default.PickMultipleAsync(options);
            if (pickedFiles == null) return results;

            foreach (var file in pickedFiles.Take(maxCount))
            {
                var result = await ConvertToFilePickerResult(file);
                if (result != null)
                    results.Add(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Multiple file picker error: {ex.Message}");
        }

        return results;
    }

    public async Task<FilePickerResult?> PickImageAsync()
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "اختر صورة",
                FileTypes = FilePickerFileType.Images
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result == null) return null;

            return await ConvertToFilePickerResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Image picker error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<FilePickerResult>> PickMultipleImagesAsync(int maxCount = 5)
    {
        var results = new List<FilePickerResult>();

        try
        {
            var options = new PickOptions
            {
                PickerTitle = "اختر صور",
                FileTypes = FilePickerFileType.Images
            };

            var pickedFiles = await FilePicker.Default.PickMultipleAsync(options);
            if (pickedFiles == null) return results;

            foreach (var file in pickedFiles.Take(maxCount))
            {
                var result = await ConvertToFilePickerResult(file);
                if (result != null)
                    results.Add(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Multiple image picker error: {ex.Message}");
        }

        return results;
    }

    private async Task<FilePickerResult?> ConvertToFilePickerResult(FileResult fileResult)
    {
        try
        {
            using var stream = await fileResult.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            return new FilePickerResult
            {
                FileName = fileResult.FileName,
                FullPath = fileResult.FullPath,
                ContentType = fileResult.ContentType ?? "application/octet-stream",
                Data = memoryStream.ToArray()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File conversion error: {ex.Message}");
            return null;
        }
    }
}
