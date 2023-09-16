using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.SecurityModels.CameraManagment
{
    [Table("CameraHistories")]
    public class CameraHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey("Camera")]
        public int CameraId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        [MaxLength(256)]
        public string Notes { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
