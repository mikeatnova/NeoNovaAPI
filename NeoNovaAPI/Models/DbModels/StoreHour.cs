using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.DbModels
{
    public class StoreHour
    {
        [Key]
        public int Id { get; set; }
        public int Day { get; set; }
        public string? OpeningTime { get; set; }
        public string? ClosingTime { get; set; }
        public int StoreId { get; set; }
    }
}
