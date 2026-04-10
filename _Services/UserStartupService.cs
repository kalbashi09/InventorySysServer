using Data;
using Models.Entities;
using Models.dto; // Ensure this is present
using Microsoft.EntityFrameworkCore;

namespace Services;

public class UserStartupService
{
    private readonly InventoryDbContext _db;
    private readonly AuthService _auth;
    private readonly LicenseService _license;

    public UserStartupService(InventoryDbContext db, AuthService auth, LicenseService license)
    {
        _db = db;
        _auth = auth;
        _license = license;
    }

    // --- STEP 1: CREATE ACCOUNT ONLY ---
    public async Task<ServiceResult<string>> InitializeNewUser(
        string userId, 
        string email, 
        string plainPassword, 
        string fullName,
        DateTime? birthdate)
    {
        // 1. Logic Check
        var userExists = await _db.Users.AnyAsync(u => u.Email == email || u.Id == userId);
        if (userExists) return ServiceResult<string>.Fail("User already exists.");

        // 2. Creation
        var user = new AppUser 
        { 
            Id = userId, 
            Email = email, 
            PasswordHash = _auth.Register(plainPassword),
            FullName = fullName,
            Birthdate = birthdate,
            SpecialCode = GenerateUniqueCode(),
            CreatedAt = DateTime.UtcNow 
        };

        _db.Users.Add(user);
        
        // Initialize License immediately so they have a trial
        _license.InitializeNewUserLicense(userId);

        await _db.SaveChangesAsync();
        
        return ServiceResult<string>.Ok(user.SpecialCode!, "Account created! Now set up your shop.");
    }

    // --- STEP 2: SETUP CATEGORIES (Separate Request) ---
    public async Task<ServiceResult<bool>> SetupShopCategories(string userId, List<string> selectedCategories)
    {
        var user = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!user) return ServiceResult<bool>.Fail("User not found.");

        foreach (var catName in selectedCategories)
        {
            // Innovation: Auto-assign type based on name keywords
            int defaultType = catName.ToLower().Contains("ingredient") || 
                              catName.ToLower().Contains("food") ? 1 : 2;

            _db.UserCategories.Add(new UserCategory
            {
                UserId = userId,
                Name = catName,
                DefaultInventoryTypeId = defaultType,
                CommonItems = new List<string>()
            });
        }

        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true, "Shop setup complete!");
    }

    public async Task<string> OnboardEmployee(EmployeeJoinDto dto)
    {
        var employer = await _db.Users
            .FirstOrDefaultAsync(u => u.SpecialCode == dto.SpecialCode && u.EmployerId == null);

        if (employer == null) throw new Exception("Employer not found or invalid code.");

        var employee = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = _auth.Register(dto.Password), 
            SpecialCode = dto.SpecialCode,
            EmployerId = employer.Id,
            Birthdate = dto.Birthdate,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(employee);
        await _db.SaveChangesAsync();
        
        return $"Employee Onboarded to {employer.FullName}'s cluster.";
    }

    private string GenerateUniqueCode()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }
}