using Data;

namespace Services;

    public class NotificationService
    {
        private readonly InventoryDbContext _db;
        // Inject your Telegram Service here later!

        public NotificationService(InventoryDbContext db)
        {
            _db = db;
        }

        // Logic for Real-time alerts (New Item / Low Stock)
        public async Task ProcessInventoryAlerts(Guid itemId)
        {
            var item = await _db.InventoryItems
                .Include(i => i.UserCategory)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null) return;

            // 1. New Item Notification
            await SendNotification(item.UserId, $"📦 New Item: {item.ItemName} added to {item.BatchName}.");

            // 2. Low Stock Logic (Threshold check)
            if (item.Quantity <= 5) // You could make this a dynamic variable later
            {
                await SendNotification(item.UserId, $"⚠️ Low Stock: {item.ItemName} only has {item.Quantity} {item.QuantityType} left!");
            }
        }

        // Logic for Expiration (Daily Summary)
        public async Task CheckExpiredItems()
        {
            var now = DateTime.UtcNow;
            var expiredItems = await _db.InventoryItems
                .Where(i => i.InStock && i.ExpiryDate.HasValue && i.ExpiryDate.Value < now)
                .ToListAsync();

            foreach (var item in expiredItems)
            {
                await SendNotification(item.UserId, $"🚨 EXPIRED: {item.ItemName} in {item.BatchName} reached its expiry on {item.ExpiryDate:MMMM dd, yyyy}.");
            }
        }

        private async Task SendNotification(string userId, string message)
        {
            // For now, we log to console. 
            // Innovative: This is where you'll call your Telegram/Push API.
            Console.WriteLine($"[NOTIF-SERVICE][User: {userId}]: {message}");
        }

        
    }

    public class ExpirationWorker : BackgroundService
    {
        private readonly IServiceProvider _services;

        public ExpirationWorker(IServiceProvider services) => _services = services;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var notifService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                    await notifService.CheckExpiredItems();
                }

                // Check every 6 hours (or 24 hours) to save CPU cycles
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }