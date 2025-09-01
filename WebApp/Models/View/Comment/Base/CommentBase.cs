namespace WebApp.Models.View.Comment.Base
{
    public class CommentBase
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = null!;
        public string Author { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
