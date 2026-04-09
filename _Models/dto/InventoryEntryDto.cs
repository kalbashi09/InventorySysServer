namespace Models.dto;

public class InventoryEntryDto
{
    public Guid Id { get; set; } // The GUID from the QR
    public string UserId { get; set; } = string.Empty;
    public int InventoryTypeId { get; set; } // 1 for Food, 2 for Material, etc.
    public int UserCategoryId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string BatchName { get; set; } = "General";
    public string QuantityType { get; set; } = "pcs";
    public decimal Quantity { get; set; }
    public DateTime? ExpiryDate { get; set; }
}