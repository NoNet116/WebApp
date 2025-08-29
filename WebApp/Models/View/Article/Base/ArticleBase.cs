using System.ComponentModel;

namespace WebApp.Models.View.Article.Base
{
    public class ArticleBase
    {
        [DisplayName("Заголовок")]
        public string Title { get; set; } = null!;
        
        public string Content { get; set; } = null!;
        
    }
}
