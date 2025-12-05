public class UserSearchViewModel
{
    public string SearchTerm { get; set; }
    public List<DBApplicationUser> Users { get; set; } = new List<DBApplicationUser>();
}