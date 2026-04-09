using System.ComponentModel.DataAnnotations;

namespace Models.Entities;

public class UserCategory
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public virtual AppUser User { get; set; } = null!;

    public string Name { get; set; } = string.Empty; // e.g., "Ingredients"

    // NEW: Link the category to a default Type
    // This allows the UI to say "In this category, we usually store Food"
    public int DefaultInventoryTypeId { get; set; }
    public virtual InventoryType DefaultInventoryType { get; set; } = null!;

    public List<string> CommonItems { get; set; } = new(); 
    
    // Navigation back to items
    public virtual ICollection<InventoryItem> Items { get; set; } = new HashSet<InventoryItem>();
}