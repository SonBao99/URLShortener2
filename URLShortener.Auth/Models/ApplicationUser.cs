using Microsoft.AspNetCore.Identity;

namespace URLShortener.Auth.Models
{
    // Custom user class extending IdentityUser
    public class ApplicationUser : IdentityUser
    {
        // You can add custom fields here if necessary, for example:
        public string FullName { get; set; }
        public string? Avatar { get; set; }
        // Other properties can go here
    }
}
