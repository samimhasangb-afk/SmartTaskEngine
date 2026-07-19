using System.ComponentModel.DataAnnotations;

namespace SmartTaskEngine.DTOs.Auth
{
    public class RegisterDto
    {
        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
