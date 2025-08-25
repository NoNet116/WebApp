using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class RoleDto
    {
        [Required]
        public required string Id { get; set; }

        [Required]
        public required string Name { get; set; }
    }
}
