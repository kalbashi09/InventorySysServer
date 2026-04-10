using System.Text.Json.Serialization;

namespace Models.dto;

public class InventoryDisplayDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string BatchLocation { get; set; } = string.Empty; // "Fridge Rack 1"
    public string QuantityDisplay { get; set; } = string.Empty; // "2.5 Liters"
    public string CategoryName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty; 

    // THE MISSING LINK:
    // This allows your Avalonia UI to bind to a "IsExpired" property
    public bool IsExpired { get; set; } 

    [JsonIgnore]
    public decimal Quantity { get; set; } 
}