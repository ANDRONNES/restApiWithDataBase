﻿using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Tutorial8.Models.DTOs;

public class CreateClientDTO
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    [Required,MinLength(8),MaxLength(15), RegularExpression("^\\d+",ErrorMessage = "The phone number must consist of digits only")] public string Telephone { get; set; }

    [Required, RegularExpression("^\\d{11}$", ErrorMessage = "Pesel must have 11 digits")]
    public string Pesel { get; set; }
}