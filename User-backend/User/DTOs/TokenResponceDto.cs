namespace User.DTOs
{
    public class TokenResponceDto
    {
        public required string AccessToken { get; set; }

        public required string RefreshToken { get; set; }

        public string Role { get; set; }
    }
}
