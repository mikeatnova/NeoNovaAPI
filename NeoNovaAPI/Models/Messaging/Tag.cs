using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.Messaging
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<MessageTag> MessageTags { get; set; } // Many-to-Many with PalantírMessage
    }
}
