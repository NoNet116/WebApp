using System.ComponentModel.DataAnnotations;

namespace WebApi.ViewModels
{
    public class EditUserRoleViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string NewRole { get; set; } = null!;
    }
}