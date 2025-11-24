using CHOICE;
using System.ComponentModel.DataAnnotations;

public class IndexViewModel
{
    public string Username { get; set; }

    public bool IsEmailConfirmed { get; set; }

    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [EmailAddress(ErrorMessage = SharedResource.EmailAddressErrorMessage)]
    public string Email { get; set; }

    [Phone]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; }

    public string? StatusMessage { get; set; }
}