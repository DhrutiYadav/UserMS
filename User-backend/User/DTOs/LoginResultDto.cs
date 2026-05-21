namespace User.DTOs
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public TokenResponceDto? Token { get; set; }
    }
}

