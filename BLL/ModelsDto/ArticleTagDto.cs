namespace BLL.ModelsDto
{
    public class ArticleTagDto
    {
        public int ArticleId { get; set; }
        public Guid TagId { get; set; }
        public DateTime CreatedAt { get; set; }

        public ArticleShortDto Article { get; set; }
        public TagDto Tag { get; set; }
    }
}