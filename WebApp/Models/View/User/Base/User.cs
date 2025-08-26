using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.View.User.Base
{
    public class User
    {
        [Required(ErrorMessage = "Заполните поле."),
            MaxLength(20), MinLength(4), DisplayName("Логин")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Заполните поле."), 
            DataType(DataType.EmailAddress), DisplayName("Почта")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Заполните поле."),
            MaxLength(50), MinLength(4), DisplayName("Имя")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Заполните поле."),
            MaxLength(50), MinLength(4), DisplayName("Фамилия")]
        public string LastName { get; set; } = null!;


        [DataType(DataType.Date), DisplayName("Дата рождения")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly BirthDate { get; set; }
    }
}
