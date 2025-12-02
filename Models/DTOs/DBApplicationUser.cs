using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public class DBApplicationUser : IdentityUser
{
    [PersonalData]
    [StringLength(100)]
    public string FirstName { get; set; }

    [PersonalData]
    [StringLength(100)]
    public string LastName { get; set; }

    [PersonalData]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}