public class TaskAccessViewModel
{
    public int TaskId { get; set; }
    public string TaskName { get; set; }
    public string OwnerUserId { get; set; }
    public List<UserAccessInfo> CurrentAccessUsers { get; set; } = new List<UserAccessInfo>();
    public List<UserAccessInfo> AvailableContacts { get; set; } = new List<UserAccessInfo>();
    public string SelectedUserId { get; set; }
}
