namespace Models.Entities;

public class UserLicense
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public virtual AppUser User { get; set; } = null!;

    // Security to prevent "Trial Spamming"
    public string? HardwareId { get; set; } 

    public bool TlUsed { get; set; } = false;
    public bool PlUsed { get; set; } = false;

    public DateTime? TlUsedDate { get; set; }
    public DateTime? PlUsedDate { get; set; }

    // Logic for Trial Expiry (15 Days)
    public bool IsTlExpired => TlUsed && TlUsedDate.HasValue 
                               && DateTime.UtcNow > TlUsedDate.Value.AddDays(15);

    // Gatekeeper: Allows entry if Trial is active OR Permanent is bought
    public bool CanAccessAccount => (TlUsed && !IsTlExpired) || PlUsed;
}