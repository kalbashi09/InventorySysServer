
using System.Text.Json.Serialization;

// ... inside the DTO ...


namespace Models.dto;

public class InventoryDisplayDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string BatchLocation { get; set; } = string.Empty; // "Fridge Rack 1"
    public string QuantityDisplay { get; set; } = string.Empty; // "2.5 Liters"
    public string CategoryName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty; // "Food", "Material", etc.

    [JsonIgnore]
    public decimal Quantity { get; set; } // This is the raw quantity for internal use, not for display
}