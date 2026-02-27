using System.Collections.Generic;
using System.Threading.Tasks;
using Khadamat.Application.DTOs;

namespace Khadamat.Application.Interfaces;

public interface IMarketplaceService
{
    Task<MarketplaceItemDto?> GetItemByIdAsync(int id);
    Task<IReadOnlyList<MarketplaceItemDto>> GetLatestItemsAsync(int count = 10);
    Task<IReadOnlyList<MarketplaceItemDto>> GetFeaturedItemsAsync(int count = 6);
    Task<IReadOnlyList<MarketplaceItemDto>> SearchItemsAsync(string? query, int? categoryId, int? subCategoryId, int? governorateId, int? cityId, string? condition, decimal? minPrice, decimal? maxPrice, string? sellerId = null, int page = 1, int pageSize = 12);
    Task<MarketplaceItemDto> CreateItemAsync(CreateMarketplaceItemRequest request, string sellerId);
    Task UpdateItemAsync(int id, CreateMarketplaceItemRequest request, string sellerId);
    Task DeleteItemAsync(int id, string sellerId);
    Task MarkAsSoldAsync(int id, string sellerId);
    Task ToggleFavoriteAsync(int id, string userId);
    Task<bool> IsFavoriteAsync(int id, string userId);
    Task ApproveItemAsync(int id, string? adminNotes = null);
    Task RejectItemAsync(int id, string? adminNotes = null);
    Task SetFeaturedAsync(int id, int days);
    Task SetPromotedAsync(int id, int days);
    Task<IReadOnlyList<MarketplaceCategoryDto>> GetCategoriesAsync();
    Task<IReadOnlyList<MarketplaceSubCategoryDto>> GetSubCategoriesAsync(int categoryId);
}
