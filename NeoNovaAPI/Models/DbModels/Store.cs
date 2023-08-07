using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.DbModels
{
    public class Store
    {
        [Key]
        public int Id { get; set; }
        public string? Image { get; set; }
        public string? Name { get; set; }
        public string? SubName { get; set; }
        public string? RegistrationType { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Hours { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string? Directions { get; set; }
    }
}
