using WebApp.Models.View.Comment.Base;

namespace WebApp.Models.View.Comment
{
    public class CommentViewModel<T>
    {
        public T Id { get; set; }
        public List<CommentBase> Comments { get; set; } = new List<CommentBase>();
    }
}
