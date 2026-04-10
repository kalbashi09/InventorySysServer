public class EmployeeJoinDto
{
    // The "Centralized Passcode" (Employer's SpecialCode)
    public string SpecialCode { get; set; } = string.Empty; 

    // Their unique name within that cluster
    public string FullName { get; set; } = string.Empty; 
    
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime? Birthdate { get; set; }
}