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
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            configuredPath = "KhadamawyImages";
        }

        string resolvedPath = null;
        if (Path.IsPathRooted(configuredPath) && Directory.Exists(configuredPath))
        {
            resolvedPath = configuredPath;
        }
        else
        {
            // Probe candidates in order:
            // 1. AppContext.BaseDirectory / configuredPath (e.g. /khadamawy/KhadamawyImages on IIS)
            // 2. Parent of BaseDirectory / configuredPath
            // 3. CurrentDirectory / configuredPath
            // 4. WebRoot parent / configuredPath
            // 5. Well-known local fallback paths (D:\maged\KhadamawyImages, D:\maged\images)
            var candidates = new System.Collections.Generic.List<string>
            {
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath)),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", configuredPath)),
                Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), configuredPath)),
                Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", configuredPath)),
                Path.GetFullPath(Path.Combine(webRoot, "..", configuredPath)),
                @"D:\maged\KhadamawyImages",
                @"D:\maged\images"
            };

            foreach (var cand in candidates)
            {
                if (Directory.Exists(cand))
                {
                    resolvedPath = cand;
                    break;
                }
            }

            // If none existed yet, default to AppContext.BaseDirectory / configuredPath
            resolvedPath ??= Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath));
        }

        // Ensure directory exists or create it automatically
        bool dirReady = false;
        try
        {
            if (!Directory.Exists(resolvedPath))
            {
                Directory.CreateDirectory(resolvedPath);
            }

            // Verify write permissions
            var probeFile = Path.Combine(resolvedPath, $".probe_{Guid.NewGuid():N}.tmp");
            File.WriteAllText(probeFile, "test");
            File.Delete(probeFile);
            dirReady = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Resolved Uploads:ImagesPath '{Path}' cannot be accessed or written to. Falling back to local wwwroot storage: {Fallback}", resolvedPath, _fallbackImagesPath);
        }

        if (dirReady)
        {
            _rootImagesPath = resolvedPath;
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
            _logger.LogInformation("Using local wwwroot storage: {Path}", _rootImagesPath);
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
