using System.ComponentModel.DataAnnotations;

namespace WebApi.ViewModels.Tags
{
    public class RegisterTagModel
    {
        [Required, MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }
}