using System;
using System.ComponentModel.DataAnnotations;

namespace Client.Models
{
    public class SignUpModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }
    }
}