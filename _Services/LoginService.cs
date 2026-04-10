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

        // --- INNOVATION: Verify the hash instead of comparing strings ---
        // This takes your plain text 'request.Password' and checks it against 'user.PasswordHash'
        bool isPasswordCorrect = _auth.Login(request.Password, user.PasswordHash);

        if (!isPasswordCorrect)
        {
            return ServiceResult<LoginResponseDto>.Fail("Invalid email or password.");
        }

        var data = new LoginResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            EmployerId = user.EmployerId,
            Message = "Login successful!"
        };

        return ServiceResult<LoginResponseDto>.Ok(data, "Welcome back!");
    }
}