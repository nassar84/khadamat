using Khadamat.Infrastructure;
using Khadamat.Application;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Khadamat.Infrastructure.Identity;
using Serilog;
using System.Security.Claims;

// 1. Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/khadamat-api-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting Khadamat Web API...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);
    builder.Host.UseSerilog();

    // ── Khadamawy Dual-Domain Environment Resolution ─────────────────────────
    // Reads Khadamawy:ActiveEnvironment (e.g. "Current" or "Production") and
    // populates ApiSettings:WebAppBaseUrl from the matching sub-section.
    // To switch domains: change Khadamawy:ActiveEnvironment in appsettings.json
    // and restart. No source-code changes are required.
    {
        var activeEnv = builder.Configuration["Khadamawy:ActiveEnvironment"] ?? "Current";
        var webBaseUrl = builder.Configuration[$"Khadamawy:{activeEnv}:WebBaseUrl"];
        var apiBaseUrl = builder.Configuration[$"Khadamawy:{activeEnv}:ApiBaseUrl"];

        if (!string.IsNullOrWhiteSpace(webBaseUrl))
            builder.Configuration["ApiSettings:WebAppBaseUrl"] = webBaseUrl;

        // ApiBaseUrl is surfaced for future use; WebAPI serves its own routes so
        // the API base is the same host in current deployment.
        if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            builder.Configuration["ApiSettings:BaseUrl"] = apiBaseUrl;

        Log.Information(
            "[KhadamawyConfig] ActiveEnvironment={Env} | WebBaseUrl={Web} | ApiBaseUrl={Api} | GooglePlayAppUrl={Play}",
            activeEnv, webBaseUrl, apiBaseUrl,
            builder.Configuration["Khadamawy:GooglePlayAppUrl"]);
    }

    // 2. Add Core services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSignalR();
    
    /*
    // Performance: Response Compression (Brotli/Gzip)
    builder.Services.AddResponseCompression(options => {
        options.EnableForHttps = true;
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    });
    */

    // 3. Clean Architecture Layers
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    // MediatR & Notification logic
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
        typeof(Khadamat.Application.DependencyInjection).Assembly,
        typeof(Khadamat.Infrastructure.DependencyInjection).Assembly
    ));
    builder.Services.AddScoped<Khadamat.Application.Interfaces.INotificationNotifier, Khadamat.WebAPI.Services.SignalRNotificationNotifier>();

    // 4. Production-Ready Authorization Policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireProvider", policy => 
            policy.RequireAuthenticatedUser()
                  .RequireClaim("is_provider", "true"));

        options.AddPolicy("RequireAdmin", policy => 
            policy.RequireRole("SystemAdmin", "SuperAdmin"));
            
        options.AddPolicy("RequireSuperAdmin", policy => 
            policy.RequireRole("SuperAdmin"));
    });

    // 5. Dynamic & Secure CORS Configuration
    builder.Services.AddCors(options =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5028" };
        
        options.AddPolicy("DefaultCors", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .SetIsOriginAllowedToAllowWildcardSubdomains();
            
            if (builder.Environment.IsDevelopment())
            {
                policy.SetIsOriginAllowed(_ => true); // Extra flexibility in dev
            }
        });
    });

    var app = builder.Build();

    // Support for Bulk Import via Command Line
    if (args.Contains("--import-services"))
    {
        Log.Information("Manual Import Mode Triggered.");
        using (var scope = app.Services.CreateScope())
        {
            await Khadamat.Infrastructure.Persistence.ServiceImporter.Run(scope.ServiceProvider, "services_import_template.csv");
        }
        return;
    }

    // 6. Automatic Database Seeding & Migration Management
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<KhadamatDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // In production, you might want to run migrations manually via CI/CD, 
            // but for this hosting environment, auto-apply is safer for updates.
            await context.Database.MigrateAsync();
            await KhadamatDbContextSeed.SeedAsync(context, userManager, roleManager);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during database migration/seeding.");
        }
    }

    // 7. Middlewares & Routing Strategy
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        // Enforce HTTPS in production
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    // app.UseResponseCompression();
    
    // Global Error Handling (Standardized)
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { Error = "An internal server error occurred.", Details = app.Environment.IsDevelopment() ? "Check logs." : null });
        });
    });

    app.UseDefaultFiles();

    // Configure Static Files to allow .apk downloads and .webp
    var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
    provider.Mappings[".apk"] = "application/vnd.android.package-archive";
    provider.Mappings[".webp"] = "image/webp";
    // Required for Digital Asset Links (TWA / Google Play Store)
    provider.Mappings[".json"] = "application/json";

    // Serve .well-known/assetlinks.json for TWA (Trusted Web Activity) verification
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
            System.IO.Path.Combine(app.Environment.WebRootPath, ".well-known")),
        RequestPath = "/.well-known",
        ContentTypeProvider = provider,
        ServeUnknownFileTypes = true
    });

    // Auto-rewrite category image requests (.png / .jpg) to .webp when the .webp file exists on disk
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(path) &&
            (path.StartsWith("/images/categories/", StringComparison.OrdinalIgnoreCase) ||
             path.StartsWith("/images/maincategories/", StringComparison.OrdinalIgnoreCase) ||
             path.StartsWith("/images/subcategories/", StringComparison.OrdinalIgnoreCase) ||
             path.StartsWith("/images/defaults/", StringComparison.OrdinalIgnoreCase)))
        {
            if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                var webpPath = System.IO.Path.ChangeExtension(path, ".webp");
                var imgStorage = context.RequestServices.GetRequiredService<Khadamat.Application.Interfaces.IImageStorageService>();
                var relativeImgPath = webpPath.Substring("/images/".Length).Replace('/', System.IO.Path.DirectorySeparatorChar);
                var physicalPath = System.IO.Path.Combine(imgStorage.RootImagesPath, relativeImgPath);
                if (System.IO.File.Exists(physicalPath))
                {
                    context.Request.Path = webpPath;
                }
                else
                {
                    var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
                    var fallbackPath = System.IO.Path.Combine(env.WebRootPath ?? "", webpPath.TrimStart('/').Replace('/', System.IO.Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(fallbackPath))
                    {
                        context.Request.Path = webpPath;
                    }
                }
            }
        }
        await next();
    });
    
    // Smart Caching Middleware for Blazor WASM & Static Assets
    app.Use(async (context, next) =>
    {
        context.Response.OnStarting(() =>
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // 1. Critical: Never cache HTML files, root, blazor.boot.json, service-worker, or APIs
            if (string.IsNullOrEmpty(path) ||
                path.Equals("/", StringComparison.Ordinal) ||
                path.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith("blazor.boot.json", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith("service-worker.js", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/v1/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
            }
            // 2. Aggressively cache immutable Blazor framework binaries (_framework/*.wasm, *.dll, etc.)
            else if (path.StartsWith("/_framework/", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
            }
            // 3. Cache static assets (images, css, js, fonts) for 30 days
            else if (path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) ||
                     path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
                     path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
                     path.StartsWith("/fonts/", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers["Cache-Control"] = "public, max-age=2592000";
            }

            return Task.CompletedTask;
        });

        await next();
    });

    // Serve uploaded images from permanent external storage directory if configured
    var imageStorage = app.Services.GetRequiredService<Khadamat.Application.Interfaces.IImageStorageService>();
    if (imageStorage.IsExternalStorageConfigured)
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(imageStorage.RootImagesPath),
            RequestPath = "/images",
            ContentTypeProvider = provider,
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            }
        });
    }

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = provider,
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
        }
    });

    app.UseCors("DefaultCors");

    app.UseAuthentication();
    app.UseAuthorization();

    // Support for hosting in sub-directory /api
    app.MapControllers();
    app.MapHub<Khadamat.WebAPI.Hubs.NotificationHub>("/notificationHub");
    app.MapHub<Khadamat.WebAPI.Hubs.ChatHub>("/chatHub");

    // Bot Redirect Middleware for Social Sharing
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value;
        if (path != null && path.StartsWith("/service/", StringComparison.OrdinalIgnoreCase))
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 2 && segments[0].Equals("service", StringComparison.OrdinalIgnoreCase) && int.TryParse(segments[1], out var serviceId))
            {
                var userAgent = context.Request.Headers["User-Agent"].ToString().ToLower();
                var isBot = userAgent.Contains("bot") || 
                            userAgent.Contains("crawler") || 
                            userAgent.Contains("spider") || 
                            userAgent.Contains("scraper") || 
                            userAgent.Contains("facebookexternalhit") || 
                            userAgent.Contains("facebot") || 
                            userAgent.Contains("twitterbot") || 
                            userAgent.Contains("whatsapp") || 
                            userAgent.Contains("telegram");

                if (isBot)
                {
                    context.Response.Redirect($"/share/service/{serviceId}");
                    return;
                }
            }
        }
        await next();
    });

    // Version check endpoint
    app.MapGet("/version", () => Results.Ok(new { version = "v2.0.0-fix", date = "2026-06-26", note = "لو شايف ده يبقى التعديلات وصلت" }));

    // Dedicated APK & Downloads handler to support all variants (khadamat.apk, Khadamawy.apk, خدماوى.apk)
    app.MapGet("/downloads/{*fileName}", (string fileName, IWebHostEnvironment env) =>
    {
        var decoded = System.Net.WebUtility.UrlDecode(fileName ?? "").Trim().TrimStart('/', '\\');
        var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var downloadsDir = Path.Combine(webRoot, "downloads");

        if (!Directory.Exists(downloadsDir))
        {
            return Results.NotFound();
        }

        string? filePath = null;
        if (!string.IsNullOrEmpty(decoded))
        {
            var directPath = Path.Combine(downloadsDir, decoded);
            if (File.Exists(directPath))
            {
                filePath = directPath;
            }
        }

        // If not found directly and it's an APK request (or empty)
        if (filePath == null && (string.IsNullOrEmpty(decoded) || decoded.EndsWith(".apk", StringComparison.OrdinalIgnoreCase)))
        {
            // Search in order of preference
            var candidateNames = new[] { "khadamat.apk", "Khadamawy.apk", "خدماوى.apk", "app.apk" };
            foreach (var cand in candidateNames)
            {
                var candPath = Path.Combine(downloadsDir, cand);
                if (File.Exists(candPath))
                {
                    filePath = candPath;
                    break;
                }
            }

            // Fallback: search for any .apk in downloads directory
            if (filePath == null)
            {
                filePath = Directory.GetFiles(downloadsDir, "*.apk").FirstOrDefault();
            }
        }

        if (filePath != null && File.Exists(filePath))
        {
            var isApk = filePath.EndsWith(".apk", StringComparison.OrdinalIgnoreCase);
            var contentType = isApk ? "application/vnd.android.package-archive" : "application/octet-stream";
            // The file always downloads to the user's mobile/device named "خدماوى.apk"
            var downloadName = isApk ? "خدماوى.apk" : Path.GetFileName(filePath);
            return Results.File(filePath, contentType, downloadName, enableRangeProcessing: true);
        }

        return Results.NotFound();
    });

    // SPA Fallback with strict no-cache headers
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
            ctx.Context.Response.Headers["Expires"] = "0";
        }
    });

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

