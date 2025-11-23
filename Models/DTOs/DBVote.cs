using System.ComponentModel.DataAnnotations;

namespace CHOICE.Models.DTOs
{
    public class DBVote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public bool IsPrivate { get; set; }
        public bool IsAHP { get; set; }

        public string Description { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDateTime { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDateTime { get; set; }

        public string UserId { get; set; }

        [Display(Name = "Allowed Users")]
        public string UsersIDs { get; set; } // Comma-separated user IDs
    }
}
