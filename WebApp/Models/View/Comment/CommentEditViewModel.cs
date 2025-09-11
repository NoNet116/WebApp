using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.View.Comment
{
    public class CommentEditViewModel
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Message { get; set; } = null!;
    }
}
