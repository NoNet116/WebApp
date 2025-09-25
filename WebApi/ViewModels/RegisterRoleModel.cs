using System.ComponentModel.DataAnnotations;

namespace WebApi.ViewModels
{
    public class RegisterRoleModel
    {
        [Required, MaxLength(50)]
        public string Name { get; set; }
    }
}