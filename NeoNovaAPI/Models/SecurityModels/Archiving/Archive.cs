using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.SecurityModels.Archiving
{
    [Table("Archives")]
    public class Archive
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public DateTime? ArchivedAt { get; set; }

        [ForeignKey("Shift")]
        public int ShiftId { get; set; }

        [ForeignKey("Tour")]
        public int TourId { get; set; }

        public DateTime RetentionDate { get; set; }
    }
}
