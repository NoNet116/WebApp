using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class Comment
    {
        public Guid Id { get; set; } = new Guid();

        [Required]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int ArticleId { get; set; }
        public string AuthorId { get; set; }

        [Required]
        public Article Article { get; set; }

        [Required]
        public User Author { get; set; }
    }
}