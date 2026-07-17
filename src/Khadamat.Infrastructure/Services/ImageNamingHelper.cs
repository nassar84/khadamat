using System;
using System.IO;

namespace Khadamat.Infrastructure.Services;

public static class ImageNamingHelper
{
    private static readonly string BasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

    /// <summary>
    /// Extract filename only from any path/URL.
    /// </summary>
    public static string? ExtractFileName(string? pathOrUrl)
    {
        if (string.IsNullOrEmpty(pathOrUrl)) return null;
        if (pathOrUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) || 
            pathOrUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return pathOrUrl;

        return Path.GetFileName(pathOrUrl);
    }

    /// <summary>
    /// Renames a temporary uploaded file to the target format.
    /// </summary>
    /// <param name="tempFileName">The temporary filename or full path/url (e.g. "123_guid.png" or "/images/services/123_guid.png")</param>
    /// <param name="folderName">The target subfolder under wwwroot/images/ (e.g. "maincategories", "categories", "subcategories", "ads", "users")</param>
    /// <param name="targetNameWithoutExtension">The target filename without extension (e.g. "cat_4", "c_3_2")</param>
    /// <returns>The final filename only (e.g. "cat_4.png"), or the original value if renaming wasn't possible/needed.</returns>
    public static string? RenameImage(string? tempFileName, string folderName, string targetNameWithoutExtension)
    {
        if (string.IsNullOrWhiteSpace(tempFileName)) return null;

        // If it's a URL or base64 data, return as is
        if (tempFileName.StartsWith("http", StringComparison.OrdinalIgnoreCase) || 
            tempFileName.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return tempFileName;

        var fileNameOnly = Path.GetFileName(tempFileName);
        
        // If the file is already named correctly, just return it
        var ext = Path.GetExtension(fileNameOnly).ToLower();
        
        // If there's no extension, default to .jpg
        if (string.IsNullOrEmpty(ext))
        {
            ext = ".jpg";
        }
        
        var targetFileName = $"{targetNameWithoutExtension}{ext}";
        if (fileNameOnly.Equals(targetFileName, StringComparison.OrdinalIgnoreCase))
            return targetFileName;

        // Try to locate the temporary file on disk
        var folderPath = Path.Combine(BasePath, "images", folderName);
        var oldFilePath = Path.Combine(folderPath, fileNameOnly);

        // If the temporary file doesn't exist there, check general/temp uploads folder
        if (!File.Exists(oldFilePath))
        {
            var generalPath = Path.Combine(BasePath, "images", "general", fileNameOnly);
            if (File.Exists(generalPath))
            {
                oldFilePath = generalPath;
            }
            else
            {
                // If it doesn't exist anywhere, it might be a seed filename or already renamed
                // Just return the filename only to keep database clean
                return fileNameOnly;
            }
        }

        try
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var newFilePath = Path.Combine(folderPath, targetFileName);

            // If a file already exists at the target path, we delete it to replace it
            if (File.Exists(newFilePath) && !newFilePath.Equals(oldFilePath, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(newFilePath);
            }

            File.Move(oldFilePath, newFilePath);
            return targetFileName;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ImageNamingHelper] Error renaming file from '{oldFilePath}' to '{targetFileName}': {ex.Message}");
            return fileNameOnly;
        }
    }
}
