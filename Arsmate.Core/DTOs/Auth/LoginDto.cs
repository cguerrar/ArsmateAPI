﻿using System.ComponentModel.DataAnnotations;

public class LoginDto
{
    [Required(ErrorMessage = "Email or username is required")]
    public string EmailOrUsername { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }

    public bool RememberMe { get; set; }

    // SIN [Required] aquí
    public string? IpAddress { get; set; } = "";
}