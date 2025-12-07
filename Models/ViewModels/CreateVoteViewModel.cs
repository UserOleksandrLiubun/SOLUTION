using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class CreateVoteViewModel
{
    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [StringLength(200)]
    [Display(Name = "Назва")]
    public string Title { get; set; }
    [Display(Name = "Приватне")]
    public bool IsPrivate { get; set; }
    [Display(Name = "Опис")]
    public string Description { get; set; }

    [Display(Name = "Дата початку")]
    public DateTime StartDateTime { get; set; } = DateTime.Now;

    [Display(Name = "Дата завершення")]
    public DateTime EndDateTime { get; set; } = DateTime.Now.AddDays(7);

    [Required]
    [Display(Name = "Учасники")]
    public List<string> UsersIDs { get; set; }
    public List<string> Alternatives { get; set; } = new();
    public List<VoteCriteriaViewModel> Criteria { get; set; } = new();
    public List<SelectListItem> Contacts { get; set; } = new();
}