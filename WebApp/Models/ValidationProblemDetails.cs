namespace WebApp.Models
{
    public class ValidationProblemDetails
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = [];
        public string TraceId { get; set; } = string.Empty;
    }

    public class ApiErrorResponse
    {
        public ValidationProblemDetails Error { get; set; } = new ValidationProblemDetails();
    }
}
