using CHOICE;
using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [Display(Name = "Ім'я користувача")]
    public string UserName { get; set; }

    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }

    [Display(Name = "Запам'ятати мене?")]
    public bool RememberMe { get; set; }
}