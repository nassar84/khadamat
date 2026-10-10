namespace Khadamat.Application.Interfaces;

/// <summary>
/// Provides secure physical path resolution and file operations for uploaded images,
/// supporting both internal (wwwroot) and external/permanent storage locations.
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Gets the root physical directory where images are stored.
    /// </summary>
    string RootImagesPath { get; }

    /// <summary>
    /// Indicates whether a dedicated external permanent storage directory is configured and active.
    /// </summary>
    bool IsExternalStorageConfigured { get; }

    /// <summary>
    /// Gets the physical path for a specific image folder (e.g., "services", "users", "ads").
    /// Ensures that folder directory exists.
    /// </summary>
    string GetFolderPath(string folder);

    /// <summary>
    /// Gets the physical path for a file inside a specific folder, with path traversal prevention.
    /// </summary>
    string GetFilePath(string folder, string filename);

    /// <summary>
    /// Checks if an image file exists in the primary storage location (or optionally in fallback wwwroot).
    /// </summary>
    bool FileExists(string folder, string filename);

    /// <summary>
    /// Deletes an image file safely if it exists.
    /// </summary>
    bool DeleteFile(string folder, string filename);

    /// <summary>
    /// Renames/moves a temporary uploaded image file to its canonical target name.
    /// </summary>
    string? RenameImage(string? tempFileName, string folderName, string targetNameWithoutExtension);
}
