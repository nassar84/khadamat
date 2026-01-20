using System.Threading.Tasks;

namespace Khadamat.Shared.Interfaces;

/// <summary>
/// File picker result
/// </summary>
public class FilePickerResult
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Interface for file picker operations
/// </summary>
public interface IFilePickerService
{
    /// <summary>
    /// Pick single file
    /// </summary>
    Task<FilePickerResult?> PickFileAsync(string[] allowedTypes);

    /// <summary>
    /// Pick multiple files
    /// </summary>
    Task<List<FilePickerResult>> PickMultipleFilesAsync(string[] allowedTypes, int maxCount = 10);

    /// <summary>
    /// Pick image file
    /// </summary>
    Task<FilePickerResult?> PickImageAsync();

    /// <summary>
    /// Pick multiple images
    /// </summary>
    Task<List<FilePickerResult>> PickMultipleImagesAsync(int maxCount = 5);
}
