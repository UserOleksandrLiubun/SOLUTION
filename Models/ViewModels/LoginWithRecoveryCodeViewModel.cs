using CHOICE;
using System.ComponentModel.DataAnnotations;

public class LoginWithRecoveryCodeViewModel
{
    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [DataType(DataType.Text)]
    [Display(Name = "Recovery Code")]
    public string RecoveryCode { get; set; }
}