using System.ComponentModel.DataAnnotations;

namespace User.Entities
{
    public class EntitieUser
    {
        public Guid Id { get; set; }
        [Required, MaxLength(50)]
        public string FirstName { get; set; }
        [Required, MaxLength(50)]
        public string LastName { get; set; }
        [Required, MaxLength(50)]
        public string UserName { get; set; }
        [ Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; }
        [MaxLength(15)]
        public string PhoneNo { get; set; }
        [Required]
        public string PasswordHash { get; set; }

        [Required, MaxLength(20)]
        public string Role { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
