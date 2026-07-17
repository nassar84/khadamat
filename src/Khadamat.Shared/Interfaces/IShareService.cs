using System.Threading.Tasks;

namespace Khadamat.Shared.Interfaces;

/// <summary>
/// Interface for sharing content
/// </summary>
public interface IShareService
{
    /// <summary>
    /// Share text content
    /// </summary>
    Task ShareTextAsync(string text, string title = "مشاركة");

    /// <summary>
    /// Share link/URL
    /// </summary>
    Task ShareLinkAsync(string url, string title = "مشاركة رابط");

    /// <summary>
    /// Share file
    /// </summary>
    Task ShareFileAsync(string filePath, string title = "مشاركة ملف");

    /// <summary>
    /// Share multiple files
    /// </summary>
    Task ShareFilesAsync(List<string> filePaths, string title = "مشاركة ملفات");

    /// <summary>
    /// Share an image (downloaded from imageUrl) along with text content (native share sheet)
    /// </summary>
    Task ShareImageWithTextAsync(string imageUrl, string text, string title = "مشاركة خدمة");
}
