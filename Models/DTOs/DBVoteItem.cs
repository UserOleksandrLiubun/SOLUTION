using System.ComponentModel.DataAnnotations;

public class DBVoteItem
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = SharedResource.RequireMessage)]
    public int DBVoteId { get; set; }

    public int? DBVoteAlternativeId { get; set; }

    [Required(ErrorMessage = SharedResource.RequireMessage)]
    public int DBVoteItemSettingsId { get; set; }

    [Display(Name = "Значення")]
    public double Value { get; set; }

    public string UserId { get; set; }
    public int AlternativePriority { get; set; }
}