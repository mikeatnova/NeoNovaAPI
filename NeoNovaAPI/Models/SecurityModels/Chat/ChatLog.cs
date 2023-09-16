using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.SecurityModels.Chat
{
    [Table("ChatLogs")]
    public class ChatLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey("SecurityUser")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(512)]
        public string Message { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
