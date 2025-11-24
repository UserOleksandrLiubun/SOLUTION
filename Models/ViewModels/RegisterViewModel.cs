using CHOICE;
using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [StringLength(50)]
    [Display(Name = "Ім'я користувача")]
    public string UserName { get; set; }

    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [EmailAddress(ErrorMessage = SharedResource.EmailAddressErrorMessage)]
    [Display(Name = "Електронна пошта")]
    public string Email { get; set; }

    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [StringLength(100, ErrorMessage = "{0} повинен містити щонайменше {2} символів і максимально {1} символів.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Підтвердьте пароль")]
    [Compare("Password", ErrorMessage = "Пароль та підтвердження пароля не збігаються.")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [Display(Name = "Ім'я")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [Display(Name = "Прізвище")]
    public string LastName { get; set; }
}