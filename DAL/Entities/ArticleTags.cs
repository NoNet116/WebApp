using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    public class ArticleTags
    {
        [Column("ArticleId")]
        public int ArticleId { get; set; }

        [Column("TagId")]
        public Guid TagId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("ArticleEntity")]
        public Article Article { get; set; }

        public Tag Tag { get; set; }
    }
}