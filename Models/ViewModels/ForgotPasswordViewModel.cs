using CHOICE;
using System.ComponentModel.DataAnnotations;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [EmailAddress(ErrorMessage = SharedResource.EmailAddressErrorMessage)]
    public string Email { get; set; }
}