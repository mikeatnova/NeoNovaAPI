﻿using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.UserModels
{
    public class CreateUserModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;  // This will be either "Admin" or "CommonUser"
    }

}
