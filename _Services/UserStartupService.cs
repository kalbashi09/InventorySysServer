using Data;
using Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class UserStartupService
{
    private readonly InventoryDbContext _db;
    private readonly AuthService _auth; // Injecting your new AuthService

    public async Task<string> InitializeNewUser(
    string userId, 
    string email, 
    string plainPassword, 
    string fullName,
    DateTime? birthdate,
    string? specialCode, 
    List<string> selectedCategories)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (userExists) return "User already exists.";

        string hashedPassword = _auth.Register(plainPassword);
        string? finalSpecialCode = null;
        string? employerId = null; // This is the actual link!

        if (!string.IsNullOrEmpty(specialCode))
        {
            // --- EMPLOYEE LOGIC ---
            // 1. Find the Employer who owns this code
            var employer = await _db.Users
                .FirstOrDefaultAsync(u => u.SpecialCode == specialCode);

            if (employer == null) return "Invalid Employer Code.";

            // 2. Set the link
            employerId = employer.Id; 
            finalSpecialCode = specialCode; 
        }
        else
        {
            // --- EMPLOYER LOGIC ---
            finalSpecialCode = GenerateUniqueCode();
            // EmployerId remains null because they ARE the boss
        }

        // 3. Create the User with the Link
        var user = new AppUser 
        { 
            Id = userId, 
            Email = email, 
            PasswordHash = hashedPassword,
            FullName = fullName,
            Birthdate = birthdate,
            SpecialCode = finalSpecialCode,
            EmployerId = employerId // The "Bridge" established!
        };
        _db.Users.Add(user);

        // 4. License & Categories (Only for Employers)
        if (employerId == null) 
        {
            _db.UserLicenses.Add(new UserLicense { Id = Guid.NewGuid(), UserId = userId });

            foreach (var catName in selectedCategories)
            {
                int defaultType = catName.ToLower().Contains("ingredient") || catName.ToLower().Contains("food") ? 1 : 2;
                _db.UserCategories.Add(new UserCategory
                {
                    UserId = userId,
                    Name = catName,
                    DefaultInventoryTypeId = defaultType,
                    CommonItems = new List<string>()
                });
            }
        }

        await _db.SaveChangesAsync();
        
        return employerId != null 
            ? $"Employee linked to {specialCode}!" 
            : $"Employer created! Code: {finalSpecialCode}";
    }

    // Simple unique code generator
    private string GenerateUniqueCode()
    {
        // Generates something like: ABC1-23DE
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }
}