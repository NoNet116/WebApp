namespace WebApp.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public int StatusCode { get; set; }
        public List<string>? Errors { get; set; }
        public bool DataIsNull => Data == null;
    }
}
