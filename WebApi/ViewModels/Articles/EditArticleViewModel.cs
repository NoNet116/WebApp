using System.ComponentModel.DataAnnotations;

namespace WebApi.ViewModels.Articles
{
    public class EditArticleViewModel
    {
        [Required(ErrorMessage = "ID статьи обязателен")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Заголовок статьи обязателен")]
        [StringLength(100, ErrorMessage = "Заголовок не должен превышать 100 символов")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Содержание статьи обязательно")]
        public string Content { get; set; } = string.Empty;

        public List<string>? Tags { get; set; }

    }
}
