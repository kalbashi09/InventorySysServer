using Microsoft.EntityFrameworkCore;
using Data;
using Models.dto;

namespace Services;

public class LoginService
{
    private readonly InventoryDbContext _db;
    private readonly AuthService _auth;

    public LoginService(InventoryDbContext db, AuthService auth)
    {
        _db = db;

        _auth = auth;
    }

    public async Task<ServiceResult<LoginResponseDto>> Authenticate(LoginRequestDto request)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null) 
            return ServiceResult<LoginResponseDto>.Fail("Invalid email or password.");

        bool isPasswordCorrect = _auth.Login(request.Password, user.PasswordHash);

        if (!isPasswordCorrect)
            return ServiceResult<LoginResponseDto>.Fail("Invalid email or password.");

        // --- LOGIC: Identify whose categories we are checking ---
        // If EmployerId is null, use user.Id (The Boss). 
        // If not null, use user.EmployerId (The Employee's Boss).
        string targetOwnerId = user.EmployerId ?? user.Id;

        // Use .AnyAsync() for high performance
        bool categoriesExist = await _db.UserCategories
            .AnyAsync(c => c.UserId == targetOwnerId);

        var data = new LoginResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            EmployerId = user.EmployerId,
            HasSetupCategories = categoriesExist, // Populate your new flag
            Message = "Login successful!"
        };

        return ServiceResult<LoginResponseDto>.Ok(data, "Welcome back!");
    }
}