using CHOICE;
using System.ComponentModel.DataAnnotations;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [DataType(DataType.Password)]
    [Display(Name = "Поточний пароль")]
    public string OldPassword { get; set; }

    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [StringLength(100, ErrorMessage = "{0} має містити щонайменше {2} та щонайбільше {1} символів.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Новий пароль")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Підтвердіть новий пароль")]
    [Compare("NewPassword", ErrorMessage = "Новий пароль та пароль підтвердження не збігаються.")]
    public string ConfirmPassword { get; set; }

    public string? StatusMessage { get; set; }
}