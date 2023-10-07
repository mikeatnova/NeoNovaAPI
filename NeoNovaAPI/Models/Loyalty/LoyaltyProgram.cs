using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.Loyalty
{
    public class LoyaltyProgram
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }
        public ICollection<LoyaltyTier> Tiers { get; set; } = new List<LoyaltyTier>();
    }
}
