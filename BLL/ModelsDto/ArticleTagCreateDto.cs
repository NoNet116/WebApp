using System.ComponentModel.DataAnnotations;

namespace BLL.ModelsDto
{
    public class ArticleTagCreateDto
    {
        [Required]
        public int ArticleId { get; set; }

        [Required]
        public Guid TagId { get; set; }
    }
}