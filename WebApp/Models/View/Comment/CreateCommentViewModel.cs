using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.View.Comment
{
    public class CreateCommentViewModel
    {
        [Required]
        public string ArticleId { get; set; } = null!;
        [Required]
        public string Message { get; set; } = null!;
    }
}
