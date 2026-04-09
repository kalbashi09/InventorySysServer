namespace Models.dto
{
    public class UserStartupDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // NEW PROFILE FIELDS
        public string FullName { get; set; } = string.Empty; // Required in User table
        public DateTime? Birthdate { get; set; } // Can be null
        public string? SpecialCode { get; set; } // Can be null

        // Initial setup for categories
        public List<string> Categories { get; set; } = new();
    }
}