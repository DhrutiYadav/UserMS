namespace User.DTOs
{
    public class ErrorResponseDto
    {
        public string Message { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public DateTime Timestamp { get; set; }

        public string? TraceId { get; set; }
    }
}
