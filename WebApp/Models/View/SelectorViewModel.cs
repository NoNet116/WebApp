using Microsoft.AspNetCore.Mvc;
using WebApp.Models.View.Tag;

namespace WebApp.Models.View
{
    public class SelectorViewModel : Controller
    {
        public List<TagViewModel> Items { get; set; }
        public string SearchTerm { get; set; }
        public string SelectedId { get; set; }
    }
}
