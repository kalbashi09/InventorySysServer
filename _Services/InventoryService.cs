using Microsoft.EntityFrameworkCore; // Required for FirstOrDefaultAsync
using Data;          // Matches your InventoryDbContext namespace
using Models.Entities; // Matches your InventoryItem namespace
using Models.dto;      // Matches your MainDto namespace    

namespace Services
{
    public class InventoryService
    {
        private readonly InventoryDbContext _db;

        // CONSTRUCTION: This is where we "Inject" the DB Manager
        public InventoryService(InventoryDbContext db)
        {
            _db = db;
        }

        public async Task<string> RegisterNewBatch(InventoryItem newItem)
        {
            // 1. Check for existing active batch with the same name/location
            // We use _db (the manager) to look into the InventoryItems table
            var existingBatch = await _db.InventoryItems
                .FirstOrDefaultAsync(x => x.BatchName == newItem.BatchName && x.InStock == true);

            if (existingBatch != null)
            {
                return "Conflict: This Batch Name is still in stock. Please clear it first!";
            }

            // 2. If existing batch is Out of Stock, or doesn't exist, proceed
            _db.InventoryItems.Add(newItem);
            
            // This pushes the changes to PostgreSQL
            await _db.SaveChangesAsync(); 
            
            return "Success: Batch Registered.";
        }

        public async Task<List<InventoryItem>> GetUserInventory(string currentUserId)
        {
            return await _db.InventoryItems
                .Where(x => x.UserId == currentUserId && x.InStock == true)
                .ToListAsync();
        }

        public async Task<string> RegisterScannedItem(InventoryEntryDto entry)
        {
            // 1. Check if the QR code is already in use
            var existingItem = await _db.InventoryItems.AnyAsync(i => i.Id == entry.Id);
            if (existingItem) 
                return "Error: This QR code is already registered to another item.";

            // 2. Check if the User exists
            var userExists = await _db.Users.AnyAsync(u => u.Id == entry.UserId);
            if (!userExists) 
                return "Error: User not found.";

            // 3. Validate that the category EXISTS and BELONGS to the user
            // We combine both checks into one efficient query
            var categoryValid = await _db.UserCategories
                .AnyAsync(c => c.Id == entry.UserCategoryId && c.UserId == entry.UserId);
                
            if (!categoryValid) 
                return "Error: Category does not exist or does not belong to this user.";

            // 3. Create the Entity
            var newItem = new InventoryItem
            {
                Id = entry.Id,
                UserId = entry.UserId,
                UserCategoryId = entry.UserCategoryId,
                InventoryTypeId = entry.InventoryTypeId, // Don't forget this!
                ItemName = entry.ItemName,
                BatchName = entry.BatchName,
                QuantityType = entry.QuantityType,
                Quantity = entry.Quantity,

                // THE CLEAN FIX: 
                // If it has a value, make it UTC. If not, just let it be null.
                ExpiryDate = entry.ExpiryDate.HasValue 
                    ? DateTime.SpecifyKind(entry.ExpiryDate.Value, DateTimeKind.Utc) 
                    : null, 

                StoredDate = DateTime.UtcNow,
                InStock = true
            };

            _db.InventoryItems.Add(newItem);
            await _db.SaveChangesAsync();

            return $"Success: {entry.ItemName} registered to Category {entry.UserCategoryId}!";
        }

        public async Task<List<InventoryDisplayDto>> GetUserInventory(string userId, int? categoryId = null, int? typeId = null)
        {
            var query = _db.InventoryItems
                .Include(i => i.UserCategory)
                .Include(i => i.InventoryType)
                .Where(i => i.UserId == userId)
                .AsQueryable();

            // 1. Filter by UserCategory (Pantry, Garage, etc.)
            if (categoryId.HasValue)
            {
                query = query.Where(i => i.UserCategoryId == categoryId.Value);
            }

            // 2. Filter by InventoryType (Food, Material, etc.)
            if (typeId.HasValue)
            {
                query = query.Where(i => i.InventoryTypeId == typeId.Value);
            }

            return await query.Select(i => new InventoryDisplayDto
            {
                ItemId = i.Id,
                // CHANGE THIS: Use i.ItemName for the display, not BatchName
                ItemName = i.ItemName, 
                QuantityDisplay = $"{i.Quantity} {i.QuantityType}",
                CategoryName = i.UserCategory.Name,
                TypeName = i.InventoryType.TypeName,
                // PRO TIP: Add BatchName to your DTO so you can see BOTH!
                BatchLocation = i.BatchName 
            }).ToListAsync();
        }
    }

    
}