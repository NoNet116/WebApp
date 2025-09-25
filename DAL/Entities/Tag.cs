using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedByUserId { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;

        public ICollection<ArticleTags> ArticleTags { get; set; } = new List<ArticleTags>();
    }
}