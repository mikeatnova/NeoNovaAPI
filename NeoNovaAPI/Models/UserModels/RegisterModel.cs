﻿using System.ComponentModel.DataAnnotations;

namespace NeoNovaAPI.Models.UserModels
{
    public class RegisterModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
