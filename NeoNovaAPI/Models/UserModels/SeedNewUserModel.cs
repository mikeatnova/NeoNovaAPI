using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.UserModels
{
    public class SeedNewUserModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
