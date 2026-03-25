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
    builder.Host.UseSerilog();

    // 2. Add Core services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSignalR();

    // 3. Clean Architecture Layers
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    // MediatR & Notification logic
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Khadamat.Infrastructure.DependencyInjection).Assembly));
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
            context.Database.Migrate();
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

    // Configure Static Files to allow .apk downloads
    var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
    provider.Mappings[".apk"] = "application/vnd.android.package-archive";
    
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = provider
    });

    app.UseCors("DefaultCors");

    app.UseAuthentication();
    app.UseAuthorization();

    // Support for hosting in sub-directory /api
    app.MapControllers();
    app.MapHub<Khadamat.WebAPI.Hubs.NotificationHub>("/notificationHub");
    app.MapHub<Khadamat.WebAPI.Hubs.ChatHub>("/chatHub");

    // SPA Fallback
    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

