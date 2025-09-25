using System.ComponentModel.DataAnnotations;

namespace WebApi.ViewModels.Comments
{
    public class EditCommentViewModel
    {
        [Required]
        public Guid CommentId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}