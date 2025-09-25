using System.ComponentModel.DataAnnotations;

namespace WebApi.ViewModels.Articles
{
    public class CreateArticleViewModel
    {
        [Required, MaxLength(50)]
        public string Title { get; set; }

        [Required, MaxLength(255)]
        public string Content { get; set; }

        [MaxLength(10)]
        public List<string>? Tags { get; set; }
    }
}