using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.View.User
{
    public class UserViewModel: Base.User
    {
        public string Id { get; set; } = null!;
        public string Role { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;

        [DataType(DataType.ImageUrl)]
        public string ProfileImage { get; set; } = string.Empty;
    }
}
