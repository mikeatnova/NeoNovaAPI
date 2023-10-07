using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.Messaging
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CommentBody { get; set; }

        [Required]
        public string Username { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("PalantirMessageId")]
        public int PalantirMessageId { get; set; }
        public PalantirMessage PalantirMessage { get; set; }
    }
}
