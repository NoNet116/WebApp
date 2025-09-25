using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class User : IdentityUser
    {
        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string? FatherName { get; set; }

        public DateOnly? BirthDate { get; set; }

        [MaxLength(255)]
        public string? Image { get; set; }

        [MaxLength(100)]
        public string Status { get; set; } = "Offline";

        public DateTime? LastActive { get; set; }

        [MaxLength(500)]
        public string? About { get; set; }
    }
}