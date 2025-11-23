using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
public class Contact
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; }
    public string ContactUserId { get; set; }
    public bool IsAccepted { get; set; }
}
public class DBVote
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    public bool IsPrivate { get; set; }

    public string Description { get; set; }

    [Display(Name = "Start Date")]
    public DateTime StartDateTime { get; set; }

    [Display(Name = "End Date")]
    public DateTime EndDateTime { get; set; }

    public string UserId { get; set; }

    [Display(Name = "Allowed Users")]
    public string UsersIDs { get; set; } // Comma-separated user IDs
}

public class DBVoteItemSettings
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DBVoteId { get; set; }

    [Required]
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

public class DBVoteAlternative
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DBVoteId { get; set; }
    public string Title { get; set; }
}

public class DBVoteItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DBVoteId { get; set; }
    
    public int? DBVoteAlternativeId { get; set; }

    [Required]
    public int DBVoteItemSettingsId { get; set; }

    [Display(Name = "Importance")]
    [Range(0, 100)]
    public double ImportanceValue { get; set; }

    [Display(Name = "Value")]
    public double Value { get; set; }

    public string UserId { get; set; }
}

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<DBTask> Tasks { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<DBVote> DBVotes { get; set; }
    public DbSet<DBVoteItemSettings> DBVoteItemSettings { get; set; }
    public DbSet<DBVoteItem> DBVoteItems { get; set; }
    public DbSet<DBVoteAlternative> DBVoteAlternative { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
}