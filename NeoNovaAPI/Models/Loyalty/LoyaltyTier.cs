using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.Loyalty
{
    public class LoyaltyTier
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int LoyaltyProgramId { get; set; }

        [ForeignKey("LoyaltyProgramId")]
        public LoyaltyProgram LoyaltyProgram { get; set; }
        [Required]
        public string Name { get; set; }
        public string? ImagePath { get; set; }
        public string? BadgeImagePath { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<SmallPerk> SmallPerks { get; set; } = new List<SmallPerk>();
        public ICollection<BigPerk> BigPerks { get; set; } = new List<BigPerk>();
        [Required]
        public int PointStart { get; set; }
        [Required]
        public int PointEnd { get; set; }
        public string? Brand { get; set; }

    }
}
