namespace Models.Entities 
{   
    public class AppUser
    {
        public string Id { get; set; } = string.Empty; // Your UserId
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? Birthdate { get; set; }
        
        // The "Join Code" generated for Employers
        public string? SpecialCode { get; set; }

        // --- MULTI-TENANT LOGIC ---

        // If this is NULL, the user is an Employer (The Boss)
        // If this has a VALUE, the user is an Employee (The Staff)
        public string? EmployerId { get; set; }

        // Navigation Property: Link to the Employer's User Account
        public virtual AppUser? Employer { get; set; }

        // Navigation Property: If this user is an Employer, these are their Staff
        public virtual ICollection<AppUser> Employees { get; set; } = new List<AppUser>();

        // Link to License (Usually only valid for Employers)
        public virtual UserLicense? License { get; set; }
        
        // Existing relationships
        public virtual ICollection<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();
        public virtual ICollection<UserCategory> Categories { get; set; } = new List<UserCategory>();
    }
}