using System.ComponentModel.DataAnnotations;

namespace User.DTOs
{
    public class LoginDto
    {
        [Required, MaxLength(255)]
        public string UserNameOrEmail { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
