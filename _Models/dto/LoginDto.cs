
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? EmployerId { get; set; } // If null, this is the Boss
    public bool IsEmployer => string.IsNullOrEmpty(EmployerId);
    public bool HasSetupCategories { get; set; }
    public string Message { get; set; } = string.Empty;
}