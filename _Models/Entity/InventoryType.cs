namespace Models.Entities
{
    public class InventoryType
    {
        public int Id { get; set; } // 1, 2, 3...
        
        // The name of the type: "Food", "Material", "Electronic", "Tool"
        public string TypeName { get; set; } = string.Empty;

        // Navigation property: One Type can have many InventoryItems
        public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    }
}