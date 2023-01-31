﻿using System.ComponentModel.DataAnnotations;

namespace AspAuthDemoApp.Feature.Authentication.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "User Name is required")]
        [StringLength(16, ErrorMessage = "Must be between 3 and 16 characters", MinimumLength = 3)]
        public string Username { get; set; } = default!;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$",
            ErrorMessage = "Must have minimum 8 characters in length, " +
            "At least one uppercase English letter, " +
            "At least one lowercase English letter, At least one digit" +
            "At least one special character")]
        public string Password { get; set; } = default!;
    }
}
