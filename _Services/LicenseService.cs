using Models.Entities;
using Microsoft.EntityFrameworkCore;
using Data; // Replace with your actual Namespace for DbContext

namespace Services;

public class LicenseService
{
    private readonly InventoryDbContext _context;

    public LicenseService(InventoryDbContext context)
    {
        _context = context;
    }

    // 1. TRIAL ACTIVATION (Self-Service)
    public async Task<string> ActivateTrial(string userId, string hwid)
    {
        var license = await _context.UserLicenses.FirstOrDefaultAsync(l => l.UserId == userId);

        if (license == null) return "User license record not found.";
        
        // Tell it like it is: Security Check
        if (license.TlUsed) return "Trial has already been used on this account.";

        // Check if this Hardware ID has been used by ANYONE else
        var hwidExists = await _context.UserLicenses.AnyAsync(l => l.HardwareId == hwid && l.TlUsed);
        if (hwidExists) return "This device has already exhausted its trial period.";

        // Activate
        license.TlUsed = true;
        license.TlUsedDate = DateTime.UtcNow;
        license.HardwareId = hwid;

        await _context.SaveChangesAsync();
        return "Success: 15-day trial started.";
    }

    // 2. PERMANENT ACTIVATION (Admin Backdoor)
    public async Task<bool> ActivatePermanentLicense(string userId)
    {
        var license = await _context.UserLicenses.FirstOrDefaultAsync(l => l.UserId == userId);

        if (license == null) return false;

        license.PlUsed = true;
        license.PlUsedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // 3. INITIALIZATION (Call this in your User Registration Service)
    public async Task InitializeNewUserLicense(string userId)
    {
        var newLicense = new UserLicense
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };

        _context.UserLicenses.Add(newLicense);
        await _context.SaveChangesAsync();
    }
}