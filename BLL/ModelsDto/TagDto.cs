using System.ComponentModel.DataAnnotations;

namespace BLL.ModelsDto
{
    public class TagDto
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string Author { get; set; } = null!;
    }
}