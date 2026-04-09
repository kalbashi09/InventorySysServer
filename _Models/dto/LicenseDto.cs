namespace Models.dto;

// For the User to start their own trial
public class TrialActivationDto
{
    public string UserId { get; set; } = string.Empty;
    public string HardwareId { get; set; } = string.Empty;
}

// For the Admin to upgrade a user (cleaner than just a URL param)
public class AdminLicenseUpdateDto
{
    public string UserId { get; set; } = string.Empty;
}