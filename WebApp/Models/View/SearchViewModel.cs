namespace WebApp.Models.View
{
    public class SearchViewModel
    {
        public string Tag { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime? DateTime { get; set; }
    }
}
