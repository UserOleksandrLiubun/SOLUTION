using System.ComponentModel.DataAnnotations;

namespace CHOICE.Models.DTOs
{
    public class DBVoteItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = SharedResource.RequireMessage)]
        public int DBVoteId { get; set; }

        public int? DBVoteAlternativeId { get; set; }

        [Required(ErrorMessage = SharedResource.RequireMessage)]
        public int DBVoteItemSettingsId { get; set; }

        [Display(Name = "Importance")]
        [Range(0, 100)]
        public double ImportanceValue { get; set; }

        [Display(Name = "Value")]
        public double Value { get; set; }

        public string UserId { get; set; }
    }
}
