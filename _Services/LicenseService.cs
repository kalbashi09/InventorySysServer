using Models.Entities;
using Microsoft.EntityFrameworkCore;
using Data;

namespace Services;

public class LicenseService
{
    private readonly InventoryDbContext _context;

    public LicenseService(InventoryDbContext context)
    {
        _context = context;
    }

    // HELPER: Direct all license traffic to the "Boss"
    private async Task<string> GetMasterOwnerId(string userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new KeyNotFoundException("User not found.");

        // If Employee, return Boss's ID. If Boss, return own ID.
        return user.EmployerId ?? user.Id;
    }

    // 1. TRIAL ACTIVATION (Redirected to Boss)
    public async Task<string> ActivateTrial(string userId, string hwid)
    {
        string ownerId = await GetMasterOwnerId(userId);

        var license = await _context.UserLicenses.FirstOrDefaultAsync(l => l.UserId == ownerId);
        if (license == null) return "Shop license record not found.";

        if (license.PlUsed) return "Permanent License is already active for this shop.";
        if (license.TlUsed) return "Trial has already been used for this shop.";

        var hwidExists = await _context.UserLicenses.AnyAsync(l => l.HardwareId == hwid && l.TlUsed);
        if (hwidExists) return "This device has already exhausted its trial period.";

        license.TlUsed = true;
        license.TlUsedDate = DateTime.UtcNow;
        license.HardwareId = hwid;

        await _context.SaveChangesAsync();
        return "Success: 15-day trial started for the whole shop.";
    }

    // 2. PERMANENT ACTIVATION (Admin Backdoor redirected to Boss)
    public async Task<bool> ActivatePermanentLicense(string userId)
    {
        string ownerId = await GetMasterOwnerId(userId);

        var license = await _context.UserLicenses.FirstOrDefaultAsync(l => l.UserId == ownerId);
        if (license == null) return false;

        license.PlUsed = true;
        license.PlUsedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // 3. CAN ACCESS CHECK (Crucial for Middleware)
    public async Task<bool> CanAccessAccount(string userId)
    {
        string ownerId = await GetMasterOwnerId(userId);

        var license = await _context.UserLicenses
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.UserId == ownerId);

        if (license == null) return false;

        // Permanent access always wins
        if (license.PlUsed) return true;

        // Trial access logic
        if (license.TlUsed && license.TlUsedDate.HasValue)
        {
            var expiryDate = license.TlUsedDate.Value.AddDays(15);
            return DateTime.UtcNow <= expiryDate;
        }

        return false;
    }

    // 4. INITIALIZATION (Required for UserStartupService)
    public void InitializeNewUserLicense(string userId)
    {
        // Note: We don't use MasterOwnerId here because this is only 
        // called during the initial Employer registration.
        var newLicense = new UserLicense
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TlUsed = false,
            PlUsed = false
        };

        _context.UserLicenses.Add(newLicense);
        // We omit SaveChanges here so it happens in the StartupService's transaction.
    }
}