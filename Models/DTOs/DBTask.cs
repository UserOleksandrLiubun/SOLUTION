using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class DBTask
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; }
    public DateTime? DueDate { get; set; } = null;
    public string Name { get; set; }
    [Column(TypeName = "nvarchar(MAX)")]
    public string Description { get; set; }
    public bool IsImportant { get; set; }
    public bool IsCompleted { get; set; }
    public string UsersIDs { get; set; }
    public string UserIDsGrantedAccess { get; set; }
}