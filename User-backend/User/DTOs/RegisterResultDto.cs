namespace User.DTOs
{
    public class RegisterResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public UserDisplayDto? User { get; set; }
    }
}
