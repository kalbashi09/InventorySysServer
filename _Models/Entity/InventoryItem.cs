namespace Models.Entities
{
    public class InventoryItem 
    {
        public Guid Id { get; set; } // The QR Code Value (Excellent use of Guid for QR!)
        
        // 1. LINK TO USER
        public string UserId { get; set; } = string.Empty;
        public virtual AppUser User { get; set; } = null!; 

        // 2. LINK TO TYPE (Food, Tool, Material)
        public int InventoryTypeId { get; set; }
        public virtual InventoryType InventoryType { get; set; } = null!;

        // 3. LINK TO CATEGORY (Fixing the CS1061 Name Mismatch)
        public int UserCategoryId { get; set; } 
        // We call it UserCategory to match your Service's .Include(i => i.UserCategory)
        public virtual UserCategory UserCategory { get; set; } = null!;

        public string ItemName { get; set; } = string.Empty; 
        public string BatchName { get; set; } = string.Empty; 
        public string QuantityType { get; set; } = string.Empty; // grams, liters
        public decimal Quantity { get; set; }
        
        public DateTime StoredDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; } 
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
        
        public bool InStock { get; set; } = true;
    }
}