using System;
using System.Collections.Generic;
using System.IO;
using Khadamat.Application.Interfaces;
using Khadamat.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Khadamat.UnitTests;

public class ImageStorageServiceTests : IDisposable
{
    private readonly string _tempTestDir;
    private readonly string _tempWwwrootDir;
    private readonly Mock<IWebHostEnvironment> _mockEnv;

    public ImageStorageServiceTests()
    {
        _tempTestDir = Path.Combine(Path.GetTempPath(), "Khadamat_Test_" + Guid.NewGuid().ToString("N"));
        _tempWwwrootDir = Path.Combine(_tempTestDir, "wwwroot");
        Directory.CreateDirectory(_tempWwwrootDir);
        Directory.CreateDirectory(Path.Combine(_tempWwwrootDir, "images"));

        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.WebRootPath).Returns(_tempWwwrootDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempTestDir))
            {
                Directory.Delete(_tempTestDir, true);
            }
        }
        catch { }
    }

    private IConfiguration CreateConfig(string? imagesPath)
    {
        var dict = new Dictionary<string, string?>();
        if (imagesPath != null)
        {
            dict["Uploads:ImagesPath"] = imagesPath;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    [Fact]
    public void When_ConfiguredPath_DoesNotExist_Throws_InvalidOperationException_WithoutSilentFallback()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempTestDir, "NonExistentFolder_12345");
        var config = CreateConfig(nonExistentPath);

        // Act & Assert: Must fail safely with a clear, actionable exception
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new ImageStorageService(config, _mockEnv.Object, NullLogger<ImageStorageService>.Instance));

        Assert.Contains("does not exist or is not accessible", ex.Message);
    }

    [Fact]
    public void When_ConfiguredPath_IsEmpty_UsesLocalWwwrootImages()
    {
        // Arrange
        var config = CreateConfig("");

        // Act
        var service = new ImageStorageService(config, _mockEnv.Object, NullLogger<ImageStorageService>.Instance);

        // Assert
        Assert.False(service.IsExternalStorageConfigured);
        Assert.Equal(Path.Combine(_tempWwwrootDir, "images"), service.RootImagesPath);
    }

    [Fact]
    public void When_ConfiguredPath_IsValid_UsesPermanentDirectory()
    {
        // Arrange
        var permanentDir = Path.Combine(_tempTestDir, "PermanentImages");
        Directory.CreateDirectory(permanentDir);
        var config = CreateConfig(permanentDir);

        // Act
        var service = new ImageStorageService(config, _mockEnv.Object, NullLogger<ImageStorageService>.Instance);

        // Assert
        Assert.True(service.IsExternalStorageConfigured);
        Assert.Equal(permanentDir, service.RootImagesPath);

        // Test folder creation inside permanent dir
        var servicesFolder = service.GetFolderPath("services");
        Assert.Equal(Path.Combine(permanentDir, "services"), servicesFolder);
        Assert.True(Directory.Exists(servicesFolder));
    }

    [Fact]
    public void GetFilePath_Sanitizes_PathTraversalAttempts()
    {
        // Arrange
        var permanentDir = Path.Combine(_tempTestDir, "PermanentImages");
        Directory.CreateDirectory(permanentDir);
        var config = CreateConfig(permanentDir);
        var service = new ImageStorageService(config, _mockEnv.Object, NullLogger<ImageStorageService>.Instance);

        // Act: attempt path traversal
        var filePath = service.GetFilePath("services", "../../evil.exe");

        // Assert: must be confined to permanentDir/services/evil.exe
        var expected = Path.Combine(permanentDir, "services", "evil.exe");
        Assert.Equal(expected, filePath);
    }

    [Fact]
    public void FileExists_FindsLegacyFiles_In_Wwwroot_When_External_Configured()
    {
        // Arrange: Legacy file exists in wwwroot/images/categories
        var legacyDir = Path.Combine(_tempWwwrootDir, "images", "categories");
        Directory.CreateDirectory(legacyDir);
        var legacyFile = Path.Combine(legacyDir, "legacy_cat.webp");
        File.WriteAllText(legacyFile, "legacy image data");

        // Permanent storage is configured separately
        var permanentDir = Path.Combine(_tempTestDir, "PermanentImages");
        Directory.CreateDirectory(permanentDir);
        var config = CreateConfig(permanentDir);
        var service = new ImageStorageService(config, _mockEnv.Object, NullLogger<ImageStorageService>.Instance);

        // Act & Assert
        Assert.True(service.FileExists("categories", "legacy_cat.webp"));
        Assert.False(service.FileExists("categories", "non_existent.webp"));
    }

    [Fact]
    public void RenameImage_MovesFile_InStorageDirectory()
    {
        // Arrange
        var permanentDir = Path.Combine(_tempTestDir, "PermanentImages");
        Directory.CreateDirectory(permanentDir);
        var config = CreateConfig(permanentDir);
        var service = new ImageStorageService(config, _mockEnv.Object, NullLogger<ImageStorageService>.Instance);

        var servicesFolder = service.GetFolderPath("services");
        var tempName = "123456_temp.webp";
        var tempFile = Path.Combine(servicesFolder, tempName);
        File.WriteAllText(tempFile, "image binary data");

        // Act
        var result = service.RenameImage(tempName, "services", "s_1_42");

        // Assert
        Assert.Equal("s_1_42.webp", result);
        Assert.False(File.Exists(tempFile));
        Assert.True(File.Exists(Path.Combine(servicesFolder, "s_1_42.webp")));
    }

    [Fact]
    public void DeleteFile_RemovesFile_FromStorageDirectory()
    {
        // Arrange
        var permanentDir = Path.Combine(_tempTestDir, "PermanentImages");
        Directory.CreateDirectory(permanentDir);
        var config = CreateConfig(permanentDir);
        var service = new ImageStorageService(config, _mockEnv.Object, NullLogger<ImageStorageService>.Instance);

        var usersFolder = service.GetFolderPath("users");
        var fileName = "u_testuser.jpg";
        var filePath = Path.Combine(usersFolder, fileName);
        File.WriteAllText(filePath, "user avatar data");
        Assert.True(File.Exists(filePath));

        // Act
        var deleted = service.DeleteFile("users", fileName);

        // Assert
        Assert.True(deleted);
        Assert.False(File.Exists(filePath));
    }
}
