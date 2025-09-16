using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.View;

public class LoginViewModel
{
    [Required(ErrorMessage = "Укажите email.")]
    [EmailAddress(ErrorMessage = "Введите корректный email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Пароль обязателен.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

}