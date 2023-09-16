using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.SecurityModels.CameraManagment
{
    [Table("CameraStatuses")]
    public class CameraStatus
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

        [Required]
        public bool IsHistorical { get; set; }
    }
}
