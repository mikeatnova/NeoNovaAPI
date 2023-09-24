using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using NeoNovaAPI.Models.SecurityModels.Reporting;

namespace NeoNovaAPI.Models.SecurityModels.ShiftManagement
{
    [Table("ShiftNotes")]
    public class ShiftNote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey("Shift")]
        public int ShiftId { get; set; }
        public virtual Shift Shift { get; set; }

        public int? NoteId { get; set; }
        public Note Note { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
