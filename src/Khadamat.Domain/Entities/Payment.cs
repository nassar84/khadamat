using System;

namespace Khadamat.Domain.Entities;

public class Payment : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public int? MarketplaceItemId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EGP";
    public string PaymentType { get; private set; } = string.Empty; // e.g., "FeaturedListing", "PromotedListing", "Subscription"
    public string Status { get; private set; } = "Pending"; // Pending, Completed, Failed
    public string? ExternalTransactionId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public virtual MarketplaceItem? MarketplaceItem { get; private set; }

    protected Payment() { }

    public Payment(string userId, decimal amount, string paymentType, int? marketplaceItemId = null, string currency = "EGP")
    {
        UserId = userId;
        Amount = amount;
        PaymentType = paymentType;
        MarketplaceItemId = marketplaceItemId;
        Currency = currency;
        CreatedAt = DateTime.UtcNow;
    }

    public void Complete(string externalTransactionId)
    {
        Status = "Completed";
        ExternalTransactionId = externalTransactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = "Failed";
        UpdatedAt = DateTime.UtcNow;
    }
}
