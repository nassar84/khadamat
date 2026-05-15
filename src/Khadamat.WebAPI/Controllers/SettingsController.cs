using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/settings")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _settingsService.GetSettingsAsync();
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut]
    public async Task<IActionResult> UpdateSettings(UpdateAppSettingsRequest request)
    {
        var result = await _settingsService.UpdateSettingsAsync(request);
        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("build-apk")]
    public async Task<IActionResult> BuildApk([FromBody] string newApiUrl)
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            var apkFilename = settings.Data?.ApkFilename ?? "khadamat.apk";

            var basePath = System.IO.Directory.GetCurrentDirectory();
            // Up to the src folder
            var srcPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(basePath, ".."));
            var mobileAppPath = System.IO.Path.Combine(srcPath, "Khadamat.MobileApp");
            var webApiWwwroot = System.IO.Path.Combine(basePath, "wwwroot");

            if (!System.IO.Directory.Exists(webApiWwwroot))
                System.IO.Directory.CreateDirectory(webApiWwwroot);

            // Update appsettings.json in MobileApp
            var appsettingsPath = System.IO.Path.Combine(mobileAppPath, "appsettings.json");
            if (System.IO.File.Exists(appsettingsPath))
            {
                var json = System.IO.File.ReadAllText(appsettingsPath);
                var jsonObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(json);
                if (jsonObj != null)
                {
                    if (jsonObj.TryGetPropertyValue("ApiSettings", out var apiSettingsNode) && apiSettingsNode is System.Text.Json.Nodes.JsonObject apiSettings)
                    {
                        apiSettings["BaseUrl"] = newApiUrl;
                        apiSettings["WebAppBaseUrl"] = newApiUrl;
                    }
                    else 
                    {
                        // Fallback if structure is different
                        jsonObj["ApiBaseUrl"] = newApiUrl;
                    }
                    System.IO.File.WriteAllText(appsettingsPath, jsonObj.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                }
            }

            // Create a background task to build and copy the APK
            _ = Task.Run(() =>
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo("dotnet", "publish -f net8.0-android -c Release")
                {
                    WorkingDirectory = mobileAppPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                if (process != null)
                {
                    process.WaitForExit(300000); // Wait up to 5 minutes

                    if (process.ExitCode == 0)
                    {
                        var apkSourceDir = System.IO.Path.Combine(mobileAppPath, "bin", "Release", "net8.0-android");
                        var apkFile = System.IO.Directory.GetFiles(apkSourceDir, "*-Signed.apk", System.IO.SearchOption.AllDirectories).FirstOrDefault();
                        if (string.IsNullOrEmpty(apkFile))
                            apkFile = System.IO.Directory.GetFiles(apkSourceDir, "*.apk", System.IO.SearchOption.AllDirectories).FirstOrDefault();

                        if (!string.IsNullOrEmpty(apkFile))
                        {
                            var targetPath = System.IO.Path.Combine(webApiWwwroot, apkFilename);
                            System.IO.File.Copy(apkFile, targetPath, true);
                        }
                    }
                }
            });

            return Ok(new { success = true, message = "جاري بناء التطبيق... سيستغرق هذا بعض الدقائق، وسيكون الرابط متاحاً للتحميل بعد الانتهاء." });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { success = false, message = "خطأ: " + ex.Message });
        }
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("upload-apk")]
    public async Task<IActionResult> UploadApk(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "لم يتم اختيار ملف" });

        if (!file.FileName.EndsWith(".apk"))
            return BadRequest(new { success = false, message = "يجب أن يكون الملف بصيغة APK" });

        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            var apkFilename = settings.Data?.ApkFilename ?? "khadamat.apk";

            var basePath = System.IO.Directory.GetCurrentDirectory();
            var webApiWwwroot = System.IO.Path.Combine(basePath, "wwwroot");

            if (!System.IO.Directory.Exists(webApiWwwroot))
                System.IO.Directory.CreateDirectory(webApiWwwroot);

            var filePath = System.IO.Path.Combine(webApiWwwroot, apkFilename);

            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { success = true, message = $"تم رفع ملف APK بنجاح ووضعه كـ {apkFilename} في رابط التحميل." });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { success = false, message = "خطأ أثناء الرفع: " + ex.Message });
        }
    }
}
