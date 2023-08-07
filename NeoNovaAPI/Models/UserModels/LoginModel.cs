using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.UserModels
{
    public class LoginModel
    {
        public string? Username { get; set; }
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
