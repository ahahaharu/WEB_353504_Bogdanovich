using System.ComponentModel.DataAnnotations;

namespace WEB_353504_Bogdanovich.UI.Models
{
    public class RegisterUserViewModel
    {
        [Required(ErrorMessage = "Поле Email обязательно для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [Display(Name = "Email (Имя пользователя)")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле Пароль обязательно для заполнения")]
        [StringLength(100, ErrorMessage = "Пароль должен содержать не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле Подтверждение пароля обязательно для заполнения")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string PasswordConfirm { get; set; }

        // Поле для загрузки файла (аватара). Используется IFormFile.
        [Display(Name = "Аватар пользователя")]
        public IFormFile? Avatar { get; set; }
    }
}
