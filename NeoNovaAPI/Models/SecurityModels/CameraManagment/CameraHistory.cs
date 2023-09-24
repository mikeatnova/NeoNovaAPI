using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using NeoNovaAPI.Models.SecurityModels.Reporting;

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

        public int? NoteId { get; set; }
        public Note Note { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
