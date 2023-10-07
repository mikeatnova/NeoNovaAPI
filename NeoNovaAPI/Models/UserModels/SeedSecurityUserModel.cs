using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.UserModels
{
    public class SeedSecurityUserModel
    {
        [MaxLength(50)]
        public string? FirstName { get; set; } // From Security User Model

        [MaxLength(50)]
        public string? LastName { get; set; } // From Security User Model

        //[Required]
        [EmailAddress]
        public string? Email { get; set; } // From ASpNetUsers

        [Required]
        public required string Role { get; set; } // From ASpNetUsers, AspNetRoles, AspNetUserRoles

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public required string Password { get; set; } // From ASpNetUsers

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // From Security User Model

        public DateTime? HiredDate { get; set; } // From Security User Model
        public string? PhoneNumber { get; set; } // From ASpNetUsers
    }
}
