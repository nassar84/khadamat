using SQLite;
using Khadamat.Application.DTOs;

namespace Khadamat.MobileApp.Services;

public class LocalDataService : Khadamat.Application.Interfaces.IOfflineDataService
{
    private SQLiteAsyncConnection? _database;

    private async Task Init()
    {
        if (_database is not null) return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "khadamat_local.db");
        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<LocalService>();
        await _database.CreateTableAsync<LocalPost>();
        await _database.CreateTableAsync<SyncAction>();
    }

    public async Task SaveServicesAsync(List<ServiceDto> services)
    {
        await Init();
        await _database!.DeleteAllAsync<LocalService>();
        var locals = services.Select(s => new LocalService
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            Price = s.Price ?? 0,
            Location = s.Location,
            CategoryName = s.CategoryName,
            ImageUrl = s.Images?.FirstOrDefault()
        }).ToList();
        await _database.InsertAllAsync(locals);
    }

    public async Task<List<ServiceDto>> GetServicesAsync()
    {
        await Init();
        var locals = await _database!.Table<LocalService>().ToListAsync();
        return locals.Select(l => new ServiceDto
        {
            Id = l.Id,
            Title = l.Title,
            Description = l.Description,
            Price = l.Price,
            Location = l.Location,
            CategoryName = l.CategoryName,
            Images = string.IsNullOrEmpty(l.ImageUrl) ? new List<string>() : new List<string> { l.ImageUrl }
        }).ToList();
    }

    public async Task AddSyncActionAsync(string action, string data)
    {
        await Init();
        await _database!.InsertAsync(new SyncAction
        {
            ActionType = action,
            PayloadJson = data,
            CreatedAt = DateTime.UtcNow
        });
    }
}

public class LocalService
{
    [PrimaryKey] public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string Location { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string? ImageUrl { get; set; }
}

public class LocalPost
{
    [PrimaryKey] public int Id { get; set; }
    public string Content { get; set; } = "";
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SyncAction
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public string ActionType { get; set; } = "";
    public string PayloadJson { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsSynced { get; set; }
}
