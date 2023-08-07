using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.DbModels
{
    public class Geofence
    {
        [Key]
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; }
    }
}
