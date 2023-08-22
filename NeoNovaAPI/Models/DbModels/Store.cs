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
        public string? MondayOpeningTime { get; set; }
        public string? MondayClosingTime { get; set; }
        public string? TuesdayOpeningTime { get; set; }
        public string? TuesdayClosingTime { get; set; }
        public string? WednesdayOpeningTime { get; set; }
        public string? WednesdayClosingTime { get; set; }
        public string? ThursdayOpeningTime { get; set; }
        public string? ThursdayClosingTime { get; set; }
        public string? FridayOpeningTime { get; set; }
        public string? FridayClosingTime { get; set; }
        public string? SaturdayOpeningTime { get; set; }
        public string? SaturdayClosingTime { get; set; }
        public string? SundayOpeningTime { get; set; }
        public string? SundayClosingTime { get; set; }
    }
}
