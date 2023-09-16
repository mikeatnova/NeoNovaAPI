using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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

        [Required]
        public string Note { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
