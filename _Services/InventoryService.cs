using Data;
using Models.Entities;
using Models.dto;

namespace Services
{
    public class InventoryService
    {
        private readonly InventoryDbContext _db;
        private readonly NotificationService _notif; // Add this

        public InventoryService(InventoryDbContext db, NotificationService notif)
        {
            _db = db;
            _notif = notif; // Inject the notification service
        }

        private async Task<string> GetMasterOwnerId(string userId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new KeyNotFoundException("User not found.");
            return user.EmployerId ?? user.Id;
        }

        public async Task<ServiceResult<InventoryItem>> RegisterScannedItem(InventoryEntryDto entry)
        {
            if (await _db.InventoryItems.AnyAsync(i => i.Id == entry.Id))
                return ServiceResult<InventoryItem>.Fail("Error: This QR code is already registered.");

            string masterId = await GetMasterOwnerId(entry.UserId);

            var categoryValid = await _db.UserCategories
                .AnyAsync(c => c.Id == entry.UserCategoryId && c.UserId == masterId);
                    
            if (!categoryValid) 
                return ServiceResult<InventoryItem>.Fail("Error: Category does not exist in this shop's inventory.");

            var newItem = new InventoryItem
            {
                Id = entry.Id,
                UserId = masterId,
                UserCategoryId = entry.UserCategoryId,
                InventoryTypeId = entry.InventoryTypeId,
                ItemName = entry.ItemName,
                BatchName = entry.BatchName,
                QuantityType = entry.QuantityType,
                Quantity = entry.Quantity,
                ExpiryDate = entry.ExpiryDate.HasValue 
                    ? DateTime.SpecifyKind(entry.ExpiryDate.Value, DateTimeKind.Utc) 
                    : null, 
                StoredDate = DateTime.UtcNow,
                InStock = true
            };

            _db.InventoryItems.Add(newItem);
            await _db.SaveChangesAsync();

            await _db.Entry(newItem).Reference(i => i.UserCategory).LoadAsync();
            await _db.Entry(newItem).Reference(i => i.InventoryType).LoadAsync();

            // 🔥 TRIGGER: Run the alert logic in the background so the user doesn't wait
            _ = Task.Run(() => _notif.ProcessInventoryAlerts(newItem.Id));

            return ServiceResult<InventoryItem>.Ok(newItem, $"Success: {entry.ItemName} registered!");
        } 

        // --- ADD THESE BACK TO InventoryService.cs ---

        public async Task<ServiceResult<List<InventoryDisplayDto>>> GetUserInventory(string userId, int? categoryId = null, int? typeId = null)
        {
            try 
            {
                string masterId = await GetMasterOwnerId(userId);

                var query = _db.InventoryItems
                    .Include(i => i.UserCategory)
                    .Include(i => i.InventoryType)
                    .Where(i => i.UserId == masterId && i.InStock == true)
                    .AsQueryable();

                if (categoryId.HasValue) query = query.Where(i => i.UserCategoryId == categoryId.Value);
                if (typeId.HasValue) query = query.Where(i => i.InventoryTypeId == typeId.Value);

                var list = await query.Select(i => new InventoryDisplayDto
                {
                    ItemId = i.Id,
                    ItemName = i.ItemName, 
                    QuantityDisplay = $"{i.Quantity} {i.QuantityType}",
                    CategoryName = i.UserCategory.Name,
                    TypeName = i.InventoryType.TypeName,
                    BatchLocation = i.BatchName,
                    IsExpired = i.ExpiryDate.HasValue && i.ExpiryDate.Value < DateTime.UtcNow
                }).ToListAsync();

                return ServiceResult<List<InventoryDisplayDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<InventoryDisplayDto>>.Fail($"Error fetching inventory: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<InventoryDisplayDto>>> GetExpiredStock(string userId)
        {
            try
            {
                string masterId = await GetMasterOwnerId(userId);
                var now = DateTime.UtcNow;

                var list = await _db.InventoryItems
                    .Include(i => i.UserCategory)
                    .Include(i => i.InventoryType)
                    .Where(i => i.UserId == masterId && i.InStock == true)
                    .Where(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value < now)
                    .OrderBy(i => i.ExpiryDate)
                    .Select(i => new InventoryDisplayDto
                    {
                        ItemId = i.Id,
                        ItemName = i.ItemName,
                        QuantityDisplay = $"{i.Quantity} {i.QuantityType}",
                        CategoryName = i.UserCategory.Name,
                        TypeName = i.InventoryType.TypeName,
                        BatchLocation = i.BatchName,
                        IsExpired = true 
                    }).ToListAsync();

                return ServiceResult<List<InventoryDisplayDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<InventoryDisplayDto>>.Fail($"Error fetching expired stock: {ex.Message}");
            }
        }

         public async Task PurgeItems(List<Guid> itemIds)
        {
            var items = await _db.InventoryItems
                .Where(i => itemIds.Contains(i.Id))
                .ToListAsync();

            foreach (var item in items)
            {
                item.InStock = false; // "Remove" from shelf
            }

            await _db.SaveChangesAsync();
        }

        public async Task<bool> IsUserAdmin(string userId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            // If EmployerId is null, it means they ARE the boss (Master Owner)
            return user != null && string.IsNullOrEmpty(user.EmployerId);
        }
    }
}

    
       
        