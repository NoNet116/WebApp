namespace BLL.ModelsDto
{
    public class ArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string DescriptionDto { get; set; } = string.Empty;
        public string AuthorId { get; set; }
        public string AuthorName { get; set; } = null!;

        public int TagsCount { get; set; }
        public int CommentsCount { get; set; }

        // Для тегов можно использовать:
        public List<string> Tags { get; set; } = new List<string>();

        // Для комментариев:
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
    }
}