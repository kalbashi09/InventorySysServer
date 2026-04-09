using BCrypt.Net;


namespace Services;

public class AuthService {

    // DURING REGISTRATION
    public string Register(string plainTextPassword)
    {
        // Hash the password with a work factor of 12 (Industry standard)
        return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, workFactor: 12);
    }

    // DURING LOGIN
    public bool Login(string inputPassword, string storedHash)
    {
        // BCrypt extracts the salt from the storedHash and compares it
        return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
    }
}