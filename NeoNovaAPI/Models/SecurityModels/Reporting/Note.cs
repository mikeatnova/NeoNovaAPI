using NeoNovaAPI.Models.SecurityModels.ShiftManagement;
using NeoNovaAPI.Models.SecurityModels.TourManagement;
using NeoNovaAPI.Models.UserModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.SecurityModels.Reporting
{
    [Table("Notes")]
    public class Note
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [ForeignKey("SecurityUser")]
        public int UserId { get; set; }

        public virtual SecurityUser User { get; set; }

        [Required]
        public string Content { get; set; }

        public string? Username { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Role { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("Shift")]
        public int? ShiftId { get; set; }

        public virtual Shift Shift { get; set; }

        [ForeignKey("Tour")]
        public int? TourId { get; set; }

        public virtual Tour Tour { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
