namespace DAL.Entities
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public string DescriptionEntity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string AuthorId { get; set; } = default!;
        public User Author { get; set; } = default!;

        public List<ArticleTags> ArticleTags { get; set; } = new List<ArticleTags>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}