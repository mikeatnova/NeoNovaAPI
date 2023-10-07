using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.Loyalty
{
    public class BigPerk
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int LoyaltyTierId { get; set; }

        [ForeignKey("LoyaltyTierId")]
        public LoyaltyTier LoyaltyTier { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
