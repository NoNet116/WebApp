namespace WebApi.ViewModels.Comments
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public string Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}