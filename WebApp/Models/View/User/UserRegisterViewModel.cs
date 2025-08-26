using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.View.User
{
    public class UserRegisterViewModel : Base.User
    {
        [Required( ErrorMessage = "Заполните поле."),
            MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов"),
            DisplayName("Пароль")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Заполните поле."),
            Compare("Password", ErrorMessage = "Пароли не совпадают"), DisplayName("Подтвердить пароль")]
        public string ConfirmPassword { get; set; } = null!;

        [DisplayName("Отчество")]
        public string? FatherName { get; set; } = string.Empty;
    }
}
