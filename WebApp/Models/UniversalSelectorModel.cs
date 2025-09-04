namespace WebApp.Models
{
    public class UniversalSelectorModel
    {
        public string FieldName { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string? Placeholder { get; set; }
        public string? SearchEndpoint { get; set; }
        public bool AllowCreate { get; set; } = true;
        public List<SelectorItem>? SelectedItems { get; set; }
    }

    public class SelectorItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
