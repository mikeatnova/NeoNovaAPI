using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.Loyalty
{
    public class UserLoyaltyStatus
    {
        [Key]
        public int Id { get; set; }
        public int CurrentPoints { get; set; } = 0;
        public int LifetimePoints { get; set; } = 0;
        public int? CurrentTierId { get; set; }

        [ForeignKey("CurrentTierId")]
        public LoyaltyTier CurrentTier { get; set; }
    }
}
