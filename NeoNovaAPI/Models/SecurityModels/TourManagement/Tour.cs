using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.SecurityModels.TourManagement
{
    [Table("Tours")]
    public class Tour
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey("Shift")]
        public int ShiftID { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        [ForeignKey("Camera")]
        public int CameraId { get; set; }

        public virtual ICollection<TourNote> TourNotes { get; set; }
    }
}
