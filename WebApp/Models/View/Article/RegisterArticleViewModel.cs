using WebApp.Models.View.Article.Base;

namespace WebApp.Models.View.Article
{
    public class RegisterArticleViewModel : ArticleBase
    {
        public List<string> Tags { get; set; } = new List<string>();
    }
}
