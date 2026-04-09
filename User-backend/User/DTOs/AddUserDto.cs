using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace User.DTOs
{
    public class AddUserDto
    {

        [Required, MaxLength(50)]
        public string FirstName { get; set; }
        [Required, MaxLength(50)]
        public string LastName { get; set; }
        [Required, MaxLength(50)]
        public string UserName { get; set; }
        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; }
        [MaxLength(15)]
        public string PhoneNo { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; } = string.Empty;

    }
}
