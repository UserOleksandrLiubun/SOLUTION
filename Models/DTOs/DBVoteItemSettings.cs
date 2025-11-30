using System.ComponentModel.DataAnnotations;

public class DBVoteItemSettings
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = SharedResource.RequireMessage)]
    public int DBVoteId { get; set; }

    [Required(ErrorMessage = SharedResource.RequireMessage)]
    [StringLength(200)]
    public string Title { get; set; }

    public string Description { get; set; }

    [Display(Name = "Важливість")]
    [Range(1, 100)]
    public double? ImportanceValue { get; set; } = 100;

    [Display(Name = "Мінімальне значення")]
    public double MinValue { get; set; } = 0;

    [Display(Name = "Крок")]
    public double StepValue { get; set; } = 0;

    [Display(Name = "Максимальне значення")]
    public double MaxValue { get; set; } = 10;
}