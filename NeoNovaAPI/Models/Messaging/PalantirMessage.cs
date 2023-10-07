using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.Messaging
{
    public class PalantirMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string MessageBody { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Realm { get; set; }

        [Required]
        [Range(1, 5)]
        public int UrgencyRating { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ModifiedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public string? Status { get; set; }

        public ICollection<MessageTag>? MessageTags { get; set; } // Many-to-Many with Tags

        public ICollection<Comment>? Comments { get; set; }
        public bool? IsArchived { get; set; }
    }
}
