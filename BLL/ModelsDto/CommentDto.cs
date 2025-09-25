namespace BLL.ModelsDto
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public string Author { get; set; }
        public string AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int ArticleId { get; set; }
    }
}