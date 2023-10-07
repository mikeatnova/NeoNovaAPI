using System.ComponentModel.DataAnnotations.Schema;

namespace NeoNovaAPI.Models.Messaging
{
    public class MessageTag
    {
        [ForeignKey("PalantirMessage")]
        public int PalantirMessageId { get; set; }
        public PalantirMessage PalantirMessage { get; set; }

        [ForeignKey("Tag")]
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
