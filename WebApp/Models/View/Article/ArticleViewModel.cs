using WebApp.Models.View.Article.Base;
using WebApp.Models.View.Comment;
using WebApp.Models.View.Comment.Base;

namespace WebApp.Models.View.Article
{
    public class ArticleViewModel :ArticleBase
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string Description { get; set; } = string.Empty;
        public string AuthorId { get; set; } = null!;
        public string AuthorName { get; set; } = null!;

        public int TagsCount { get; set; }
        public int CommentsCount { get; set; }

        public List<string> Tags { get; set; }  = new List<string>();

        public List<CommentBase> Comments { get; set; } = new ();
    }
}
