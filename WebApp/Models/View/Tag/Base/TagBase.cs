using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.View.Tag.Base
{
    public class TagBase
    {

        public string Description { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = null!;

    }
}
