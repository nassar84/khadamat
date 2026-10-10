using System;
using System.IO;
using Khadamat.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Khadamat.Infrastructure.Services;

public class ImageStorageService : IImageStorageService
{
    private readonly string _rootImagesPath;
    private readonly string _fallbackImagesPath;
    private readonly bool _isExternalStorageConfigured;
    private readonly ILogger<ImageStorageService> _logger;

    public string RootImagesPath => _rootImagesPath;
    public bool IsExternalStorageConfigured => _isExternalStorageConfigured;

    public ImageStorageService(
        IConfiguration configuration,
        IWebHostEnvironment env,
        ILogger<ImageStorageService> logger)
    {
        _logger = logger;

        var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        _fallbackImagesPath = Path.Combine(webRoot, "images");

        var configuredPath = configuration["Uploads:ImagesPath"]?.Trim();

        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            // Verify that the configured permanent directory exists
            if (!Directory.Exists(configuredPath))
            {
                var errorMsg = $"CRITICAL CONFIGURATION ERROR: The configured Uploads:ImagesPath '{configuredPath}' does not exist or is not accessible. " +
                               $"To prevent data loss during deployment, the application will not start with an invalid storage path. " +
                               $"Please ensure the directory exists and the IIS Application Pool identity has Read/Write permissions.";
                _logger.LogCritical(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            // Verify write permissions by probing the directory
            try
            {
                var probeFile = Path.Combine(configuredPath, $".probe_{Guid.NewGuid():N}.tmp");
                File.WriteAllText(probeFile, "test");
                File.Delete(probeFile);
            }
            catch (Exception ex)
            {
                var errorMsg = $"CRITICAL PERMISSION ERROR: The configured Uploads:ImagesPath '{configuredPath}' is not writable. " +
                               $"Please grant Modify/Write permissions to the IIS Application Pool identity. Error: {ex.Message}";
                _logger.LogCritical(ex, errorMsg);
                throw new InvalidOperationException(errorMsg, ex);
            }

            _rootImagesPath = configuredPath;
            _isExternalStorageConfigured = true;
            _logger.LogInformation("Permanent image storage successfully configured at: {Path}", _rootImagesPath);
        }
        else
        {
            _rootImagesPath = _fallbackImagesPath;
            _isExternalStorageConfigured = false;
            
            if (!Directory.Exists(_rootImagesPath))
            {
                Directory.CreateDirectory(_rootImagesPath);
            }
            _logger.LogInformation("No external Uploads:ImagesPath configured. Using local wwwroot storage: {Path}", _rootImagesPath);
        }

        // Initialize ImageNamingHelper with this service instance
        ImageNamingHelper.Configure(this);
    }

    public string GetFolderPath(string folder)
    {
        folder = SanitizeFolder(folder);
        var path = string.IsNullOrEmpty(folder) 
            ? _rootImagesPath 
            : Path.Combine(_rootImagesPath, folder);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    public string GetFilePath(string folder, string filename)
    {
        var cleanFile = SanitizeFileName(filename);
        var folderPath = GetFolderPath(folder);
        return Path.Combine(folderPath, cleanFile);
    }

    public bool FileExists(string folder, string filename)
    {
        if (string.IsNullOrWhiteSpace(filename)) return false;

        var cleanFile = SanitizeFileName(filename);
        var primaryPath = Path.Combine(GetFolderPath(folder), cleanFile);
        if (File.Exists(primaryPath)) return true;

        // If external storage is active, check fallback wwwroot for legacy unmigrated assets
        if (_isExternalStorageConfigured)
        {
            var cleanFolder = SanitizeFolder(folder);
            var fallbackPath = string.IsNullOrEmpty(cleanFolder)
                ? Path.Combine(_fallbackImagesPath, cleanFile)
                : Path.Combine(_fallbackImagesPath, cleanFolder, cleanFile);

            if (File.Exists(fallbackPath)) return true;
        }

        return false;
    }

    public bool DeleteFile(string folder, string filename)
    {
        if (string.IsNullOrWhiteSpace(filename)) return false;

        var cleanFile = SanitizeFileName(filename);
        bool deleted = false;

        try
        {
            var primaryPath = Path.Combine(GetFolderPath(folder), cleanFile);
            if (File.Exists(primaryPath))
            {
                File.Delete(primaryPath);
                deleted = true;
            }

            // Also remove from fallback if present
            if (_isExternalStorageConfigured)
            {
                var cleanFolder = SanitizeFolder(folder);
                var fallbackPath = string.IsNullOrEmpty(cleanFolder)
                    ? Path.Combine(_fallbackImagesPath, cleanFile)
                    : Path.Combine(_fallbackImagesPath, cleanFolder, cleanFile);

                if (File.Exists(fallbackPath))
                {
                    File.Delete(fallbackPath);
                    deleted = true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file '{Filename}' in folder '{Folder}'", filename, folder);
        }

        return deleted;
    }

    public string? RenameImage(string? tempFileName, string folderName, string targetNameWithoutExtension)
    {
        if (string.IsNullOrWhiteSpace(tempFileName)) return null;

        if (tempFileName.StartsWith("http", StringComparison.OrdinalIgnoreCase) || 
            tempFileName.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return tempFileName;

        var fileNameOnly = Path.GetFileName(tempFileName);
        var ext = Path.GetExtension(fileNameOnly).ToLower();
        if (string.IsNullOrEmpty(ext))
        {
            ext = ".jpg";
        }

        var targetFileName = $"{targetNameWithoutExtension}{ext}";
        if (fileNameOnly.Equals(targetFileName, StringComparison.OrdinalIgnoreCase))
            return targetFileName;

        var targetFolder = GetFolderPath(folderName);
        var oldFilePath = Path.Combine(targetFolder, fileNameOnly);

        // If not in target folder, check other upload folders
        if (!File.Exists(oldFilePath))
        {
            string[] otherFolders = { "general", "users", "services", "marketplace", "ads", "categories", "subcategories", "maincategories" };
            bool found = false;
            foreach (var f in otherFolders)
            {
                var cand = Path.Combine(GetFolderPath(f), fileNameOnly);
                if (File.Exists(cand))
                {
                    oldFilePath = cand;
                    found = true;
                    break;
                }
            }

            // If still not found, check fallback wwwroot (for files uploaded before migration)
            if (!found && _isExternalStorageConfigured)
            {
                foreach (var f in otherFolders)
                {
                    var candFallback = Path.Combine(_fallbackImagesPath, f, fileNameOnly);
                    if (File.Exists(candFallback))
                    {
                        oldFilePath = candFallback;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                return fileNameOnly;
            }
        }

        try
        {
            var newFilePath = Path.Combine(targetFolder, targetFileName);

            if (File.Exists(newFilePath) && !newFilePath.Equals(oldFilePath, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(newFilePath);
            }

            File.Move(oldFilePath, newFilePath);
            return targetFileName;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error renaming file from '{OldPath}' to '{TargetName}'", oldFilePath, targetFileName);
            return fileNameOnly;
        }
    }

    private static string SanitizeFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder)) return string.Empty;
        return folder.Trim().Replace("/", "").Replace("\\", "").Replace("..", "");
    }

    private static string SanitizeFileName(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename)) return string.Empty;
        var clean = Path.GetFileName(filename);
        return clean.Replace("..", "").Trim();
    }
}
