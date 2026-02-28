using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Exceptions;
using Khadamat.Infrastructure.Identity;

namespace Khadamat.Infrastructure.Services;

public class MarketplaceService : IMarketplaceService
{
    private readonly KhadamatDbContext _context;

    public MarketplaceService(KhadamatDbContext context)
    {
        _context = context;
    }

    public async Task<MarketplaceItemDto?> GetItemByIdAsync(int id)
    {
        var item = await _context.MarketplaceItems
            .Include(m => m.Category)
            .Include(m => m.SubCategory)
            .Include(m => m.City)
            .Include(m => m.Images)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null) return null;

        var seller = await _context.Users.FindAsync(item.SellerId) as ApplicationUser;
        item.IncrementViews();
        await _context.SaveChangesAsync();

        return MapToDto(item, seller?.FullName);
    }

    public async Task<IReadOnlyList<MarketplaceItemDto>> GetLatestItemsAsync(int count = 10)
    {
        var items = await _context.MarketplaceItems
            .Include(m => m.Category)
            .Include(m => m.Images)
            .Where(m => m.Approved && m.ItemStatus == "Available")
            .OrderByDescending(m => m.IsPromoted)
            .ThenByDescending(m => m.CreatedAt)
            .Take(count)
            .ToListAsync();

        return await EnrichWithSellerNamesAsync(items);
    }

    public async Task<IReadOnlyList<MarketplaceItemDto>> GetFeaturedItemsAsync(int count = 6)
    {
        var items = await _context.MarketplaceItems
            .Include(m => m.Category)
            .Include(m => m.Images)
            .Where(m => m.Approved && m.ItemStatus == "Available" && m.IsFeatured)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .ToListAsync();

        return await EnrichWithSellerNamesAsync(items);
    }

    public async Task<IReadOnlyList<MarketplaceItemDto>> SearchItemsAsync(
        string? query, 
        int? categoryId, 
        int? subCategoryId,
        int? governorateId,
        int? cityId, 
        string? condition, 
        decimal? minPrice, 
        decimal? maxPrice, 
        string? sellerId = null,
        string? sortBy = "date_desc",
        int page = 1, 
        int pageSize = 12)
    {
        var dbQuery = _context.MarketplaceItems
            .Include(m => m.Category)
            .Include(m => m.Images)
            .AsQueryable();

        // If searching by sellerId, we might want to see unapproved items or sold items
        if (!string.IsNullOrEmpty(sellerId))
        {
            dbQuery = dbQuery.Where(m => m.SellerId == sellerId);
        }
        else
        {
            dbQuery = dbQuery.Where(m => m.Approved && m.ItemStatus == "Available");
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            dbQuery = dbQuery.Where(m => m.Title.Contains(query) || m.Description.Contains(query));
        }

        if (categoryId.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.CategoryId == categoryId.Value);
        }

        if (subCategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.SubCategoryId == subCategoryId.Value);
        }

        if (governorateId.HasValue && !cityId.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.City != null && m.City.GovernorateId == governorateId.Value);
        }

        if (cityId.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.CityId == cityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(condition))
        {
            dbQuery = dbQuery.Where(m => m.Condition == condition);
        }

        if (minPrice.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.Price <= maxPrice.Value);
        }

        IOrderedQueryable<MarketplaceItem> orderedQuery;
        
        switch (sortBy?.ToLower())
        {
            case "price_asc":
                orderedQuery = dbQuery.OrderByDescending(m => m.IsPromoted).ThenBy(m => m.Price);
                break;
            case "price_desc":
                orderedQuery = dbQuery.OrderByDescending(m => m.IsPromoted).ThenByDescending(m => m.Price);
                break;
            case "views_desc":
                orderedQuery = dbQuery.OrderByDescending(m => m.IsPromoted).ThenByDescending(m => m.ViewsCount);
                break;
            case "date_asc":
                orderedQuery = dbQuery.OrderByDescending(m => m.IsPromoted).ThenBy(m => m.CreatedAt);
                break;
            case "date_desc":
            default:
                orderedQuery = dbQuery.OrderByDescending(m => m.IsPromoted).ThenByDescending(m => m.CreatedAt);
                break;
        }

        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await EnrichWithSellerNamesAsync(items);
    }

    private async Task<IReadOnlyList<MarketplaceItemDto>> EnrichWithSellerNamesAsync(List<MarketplaceItem> items)
    {
        var sellerIds = items.Select(i => i.SellerId).Distinct().ToList();
        var sellers = await _context.Users
            .Where(u => sellerIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id, u => u.FullName);

        return items.Select(item => MapToDto(item, sellers.TryGetValue(item.SellerId, out var name) ? name : null)).ToList();
    }

    public async Task<MarketplaceItemDto> CreateItemAsync(CreateMarketplaceItemRequest request, string sellerId)
    {
        var item = new MarketplaceItem(
            request.Title,
            request.Description,
            request.Price,
            sellerId,
            request.CategoryId,
            request.ContactPhone,
            request.SubCategoryId,
            request.CityId,
            request.Condition
        );

        _context.MarketplaceItems.Add(item);
        await _context.SaveChangesAsync();

        if (request.Images != null && request.Images.Any())
        {
            for (int i = 0; i < request.Images.Count; i++)
            {
                var imageUrl = await SaveImage(request.Images[i], item.Id, i);
                _context.MarketplaceImages.Add(new MarketplaceImage(item.Id, imageUrl, i, i == 0));
            }
            await _context.SaveChangesAsync();
        }

        return MapToDto(item);
    }

    private async Task<string> SaveImage(string base64Data, int itemId, int index)
    {
        // Simple base64 saving logic (for production, use a more robust storage service)
        try
        {
            var folderPath = Path.Combine("wwwroot", "uploads", "marketplace");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = $"item_{itemId}_{index}_{DateTime.UtcNow.Ticks}.jpg";
            var filePath = Path.Combine(folderPath, fileName);

            var data = base64Data.Contains(",") ? base64Data.Split(',')[1] : base64Data;
            var bytes = Convert.FromBase64String(data);
            await File.WriteAllBytesAsync(filePath, bytes);

            return $"/uploads/marketplace/{fileName}";
        }
        catch
        {
            return "/images/defaults/default-product.png";
        }
    }

    public async Task UpdateItemAsync(int id, CreateMarketplaceItemRequest request, string sellerId)
    {
        var item = await _context.MarketplaceItems.FindAsync(id);
        if (item == null) throw new BusinessRuleException("Item not found");
        if (item.SellerId != sellerId) throw new BusinessRuleException("Unauthorized");

        item.UpdateItem(
            request.Title,
            request.Description,
            request.Price,
            request.ContactPhone,
            request.Condition,
            request.CategoryId,
            request.SubCategoryId,
            request.CityId
        );

        await _context.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int id, string sellerId)
    {
        var item = await _context.MarketplaceItems.FindAsync(id);
        if (item == null) return;
        if (item.SellerId != sellerId) throw new BusinessRuleException("Unauthorized");

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.DeletedBy = sellerId;

        await _context.SaveChangesAsync();
    }

    public async Task MarkAsSoldAsync(int id, string sellerId)
    {
        await ChangeItemStatusAsync(id, "Sold", sellerId);
    }

    public async Task ChangeItemStatusAsync(int id, string status, string actorId, bool isAdmin = false)
    {
        var item = await _context.MarketplaceItems.FindAsync(id);
        if (item == null) throw new BusinessRuleException("Item not found");
        if (!isAdmin && item.SellerId != actorId)
            throw new BusinessRuleException("Unauthorized");

        switch (status)
        {
            case "Available":
                item.MarkAsAvailable();
                break;
            case "Sold":
                item.MarkAsSold();
                break;
            case "Expired":
                item.MarkAsExpired();
                break;
            case "Cancelled":
                item.MarkAsCancelled();
                break;
            case "Locked":
                item.Lock();
                break;
            default:
                throw new BusinessRuleException("Invalid status");
        }
        await _context.SaveChangesAsync();
    }

    public async Task LogItemViewAsync(int id, string userId)
    {
        var view = await _context.MarketplaceItemViews
            .FirstOrDefaultAsync(v => v.MarketplaceItemId == id && v.UserId == userId);

        if (view == null)
        {
            _context.MarketplaceItemViews.Add(new MarketplaceItemView
            {
                MarketplaceItemId = id,
                UserId = userId,
                ViewedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetItemViewsCountAsync(int id, string sellerId)
    {
        var item = await _context.MarketplaceItems.FindAsync(id);
        if (item == null || item.SellerId != sellerId) return 0;
        
        return await _context.MarketplaceItemViews.CountAsync(v => v.MarketplaceItemId == id);
    }

    public async Task ToggleFavoriteAsync(int id, string userId)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.MarketplaceItemId == id && f.UserId == userId);

        if (favorite != null)
        {
            _context.Favorites.Remove(favorite);
        }
        else
        {
            _context.Favorites.Add(new Favorite(userId, marketplaceItemId: id));
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsFavoriteAsync(int id, string userId)
    {
        return await _context.Favorites
            .AnyAsync(f => f.MarketplaceItemId == id && f.UserId == userId);
    }

    public async Task ApproveItemAsync(int id, DateTime? startDate = null, DateTime? endDate = null, string? adminNotes = null)
    {
        var item = await _context.MarketplaceItems.FindAsync(id);
        if (item == null) return;

        // Get marketplace settings for default duration
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        int defaultDays = settings?.MarketplaceDefaultListingDays ?? 30;

        item.Approve(startDate, endDate, adminNotes, defaultDays);
        await _context.SaveChangesAsync();
    }

    public async Task RejectItemAsync(int id, string? adminNotes = null)
    {
        var item = await _context.MarketplaceItems.FindAsync(id);
        if (item == null) return;

        item.Reject(adminNotes);
        await _context.SaveChangesAsync();
    }

    public async Task SetFeaturedAsync(int id, int days)
    {
        var item = await _context.MarketplaceItems.FindAsync(id);
        if (item == null) return;

        item.SetFeatured(days);
        await _context.SaveChangesAsync();
    }

    public async Task SetPromotedAsync(int id, int days)
    {
        var item = await _context.MarketplaceItems.FindAsync(id);
        if (item == null) return;

        item.SetPromoted(days);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<MarketplaceCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _context.MarketplaceCategories
            .Include(c => c.SubCategories)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return categories.Select(c => new MarketplaceCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Icon = c.Icon,
            ImageUrl = c.ImageUrl,
            DisplayOrder = c.DisplayOrder,
            SubCategories = c.SubCategories.Select(s => new MarketplaceSubCategoryDto
            {
                Id = s.Id,
                CategoryId = s.CategoryId,
                Name = s.Name,
                ImageUrl = s.ImageUrl,
                DisplayOrder = s.DisplayOrder
            }).OrderBy(s => s.DisplayOrder).ToList()
        }).ToList();
    }

    public async Task<IReadOnlyList<MarketplaceSubCategoryDto>> GetSubCategoriesAsync(int categoryId)
    {
        var subCategories = await _context.MarketplaceSubCategories
            .Where(s => s.CategoryId == categoryId)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

        return subCategories.Select(s => new MarketplaceSubCategoryDto
        {
            Id = s.Id,
            CategoryId = s.CategoryId,
            Name = s.Name,
            ImageUrl = s.ImageUrl,
            DisplayOrder = s.DisplayOrder
        }).ToList();
    }

    public async Task<MarketplaceSettingsDto> GetMarketplaceSettingsAsync()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        return new MarketplaceSettingsDto
        {
            DefaultListingDays = settings?.MarketplaceDefaultListingDays ?? 30,
            MaxListingsPerUser = settings?.MarketplaceMaxListingsPerUser ?? 10,
            RequireApproval = settings?.MarketplaceRequireApproval ?? true,
            AutoExpire = settings?.MarketplaceAutoExpire ?? true
        };
    }

    public async Task LockItemAsync(int id, string actorId, bool isAdmin = false)
    {
        var item = await _context.MarketplaceItems.FindAsync(id);
        if (item == null) throw new BusinessRuleException("Item not found");
        if (!isAdmin && item.SellerId != actorId)
            throw new BusinessRuleException("Unauthorized");

        item.Lock();
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetActiveListingsCountForUserAsync(string userId)
    {
        return await _context.MarketplaceItems
            .CountAsync(m => m.SellerId == userId && m.ItemStatus == "Available" && m.Approved);
    }

    public async Task<IReadOnlyList<MarketplaceItemDto>> GetAllItemsAdminAsync(int page = 1, int pageSize = 200)
    {
        var items = await _context.MarketplaceItems
            .Include(m => m.Category)
            .Include(m => m.Images)
            .Include(m => m.City)
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await EnrichWithSellerNamesAsync(items);
    }

    private MarketplaceItemDto MapToDto(MarketplaceItem item, string? sellerName = null)
    {
        return new MarketplaceItemDto
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            Price = item.Price,
            Currency = item.Currency,
            Condition = item.Condition,
            ItemStatus = item.ItemStatus,
            ContactPhone = item.ContactPhone,
            SellerId = item.SellerId,
            SellerName = sellerName ?? "",
            CategoryId = item.CategoryId,
            CategoryName = item.Category?.Name ?? "",
            SubCategoryId = item.SubCategoryId,
            SubCategoryName = item.SubCategory?.Name,
            CityId = item.CityId,
            CityName = item.City?.City_Name_AR,
            ViewsCount = item.ViewsCount,
            IsFeatured = item.IsFeatured && (item.FeaturedUntil == null || item.FeaturedUntil > DateTime.UtcNow),
            IsPromoted = item.IsPromoted && (item.PromotedUntil == null || item.PromotedUntil > DateTime.UtcNow),
            FeaturedUntil = item.FeaturedUntil,
            PromotedUntil = item.PromotedUntil,
            CreatedAt = item.CreatedAt,
            ListedAt = item.ListedAt,
            StartDate = item.StartDate,
            EndDate = item.EndDate,
            SoldDate = item.SoldDate,
            Approved = item.Approved,
            Images = item.Images.Select(i => new MarketplaceImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsMain = i.IsMain,
                DisplayOrder = i.DisplayOrder
            }).OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder).ToList()
        };
    }
}
