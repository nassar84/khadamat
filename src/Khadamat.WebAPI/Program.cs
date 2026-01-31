using Khadamat.Infrastructure;
using Khadamat.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Khadamat.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// Clean Architecture Layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddScoped<Khadamat.Application.Interfaces.INotificationNotifier, Khadamat.WebAPI.Services.SignalRNotificationNotifier>();

// Authorization Policies
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.WithOrigins("http://localhost:5028", "https://localhost:7082", "http://localhost:5144") // Adjust for dev/prod
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<KhadamatDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply migrations automatically
        context.Database.Migrate();
        // Seed data
        await KhadamatDbContextSeed.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// app.UseHttpsRedirection();

// Serve Static Files for the Frontend
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<Khadamat.WebAPI.Hubs.NotificationHub>("/notificationHub");
app.MapHub<Khadamat.WebAPI.Hubs.ChatHub>("/chatHub");

// Fallback to index.html for SPA-like behavior
app.MapFallbackToFile("index.html");

app.Run();
