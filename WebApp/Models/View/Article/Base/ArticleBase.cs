namespace WebApp.Models.View.Article.Base
{
    public class ArticleBase
    {
        public int Id { get; set; } 
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string Description { get; set; } = string.Empty;
        public string AuthorId { get; set; } = null!;
        public string AuthorName { get; set; } = null!;

        public int TagsCount { get; set; }
        public int CommentsCount { get; set; }


    }
}
