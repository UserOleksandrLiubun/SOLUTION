using System.ComponentModel.DataAnnotations;

namespace GroupChoice.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
