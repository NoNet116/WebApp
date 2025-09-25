using System.ComponentModel.DataAnnotations;

namespace WebApi.ViewModels.Comments
{
    public class CreateCommentViewModel
    {
        public int ArticleId { get; set; }

        [MaxLength(250)]
        public string Message { get; set; }
    }
}