using System.ComponentModel.DataAnnotations;

namespace GroupChoice.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
