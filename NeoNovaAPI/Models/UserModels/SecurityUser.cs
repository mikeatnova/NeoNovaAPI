using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.UserModels
{
    [Table("SecurityUsers")]
    public class SecurityUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ModifiedAt { get; set; }

        public DateTime? HiredDate { get; set; }

        public DateTime? TerminationDate { get; set; }

        // Navigation property for 1-to-1 relationship with IdentityUser
        [ForeignKey("IdentityUser")]
        public string IdentityUserId { get; set; }

        public IdentityUser IdentityUser { get; set; }
    }
}
