using System.ComponentModel.DataAnnotations;

namespace CHOICE.Models.DTOs
{
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

        [Display(Name = "Importance")]
        [Range(1, 100)]
        public double ImportanceValue { get; set; } = 100;

        [Display(Name = "Min Value")]
        public double MinValue { get; set; } = 0;

        [Display(Name = "Step Value")]
        public double StepValue { get; set; } = 0;

        [Display(Name = "Max Value")]
        public double MaxValue { get; set; } = 10;
    }
}
