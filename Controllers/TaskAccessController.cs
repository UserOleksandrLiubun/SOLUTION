using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class TaskAccessController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<DBApplicationUser> _userManager;

    public TaskAccessController(ApplicationDbContext context, UserManager<DBApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int taskId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null)
        {
            return NotFound();
        }

        var model = await CreateTaskAccessViewModel(task);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GrantAccess(int taskId, string selectedUserId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(selectedUserId))
        {
            var currentAccessUsers = task.UserIDsGrantedAccess?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

            if (!currentAccessUsers.Contains(selectedUserId))
            {
                currentAccessUsers.Add(selectedUserId);
                task.UserIDsGrantedAccess = string.Join(",", currentAccessUsers);
                await _context.SaveChangesAsync();
            }
        }

        var model = await CreateTaskAccessViewModel(task);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAccess(int taskId, string userId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null || task.UserId == userId)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(userId))
        {
            var currentAccessUsers = task.UserIDsGrantedAccess?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            currentAccessUsers.Remove(userId);
            task.UserIDsGrantedAccess = currentAccessUsers.Any() ? string.Join(",", currentAccessUsers) : null;
            await _context.SaveChangesAsync();
        }

        var model = await CreateTaskAccessViewModel(task);
        return View("Index", model);
    }

    private async Task<TaskAccessViewModel> CreateTaskAccessViewModel(DBTask task)
    {
        var model = new TaskAccessViewModel
        {
            TaskId = task.Id,
            TaskName = task.Name,
            OwnerUserId = task.UserId
        };

        // Get current users with access
        if (!string.IsNullOrEmpty(task.UserIDsGrantedAccess))
        {
            var userIds = task.UserIDsGrantedAccess.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var userId in userIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                model.CurrentAccessUsers.Add(new UserAccessInfo
                {
                    UserId = userId,
                    DisplayName = $"{user?.FirstName} {user?.LastName} / {user?.UserName}"
                });
            }
        }

        // Get available contacts for the task owner
        var contacts = await _context.Contacts
            .Where(c => (c.UserId == task.UserId || c.ContactUserId == task.UserId) && c.IsAccepted)
            .ToListAsync();

        var currentAccessUserIds = model.CurrentAccessUsers.Select(u => u.UserId).ToList();

        foreach (var contact in contacts)
        {
            if (!currentAccessUserIds.Contains(contact.ContactUserId) && (contact.ContactUserId != task.UserId) || !currentAccessUserIds.Contains(contact.UserId) && (contact.UserId != task.UserId))
            {
                var user = await _userManager.FindByIdAsync(contact.ContactUserId == task.UserId ? contact.UserId : contact.ContactUserId);
                model.AvailableContacts.Add(new UserAccessInfo
                {
                    UserId = user.Id,
                    DisplayName = $"{user?.FirstName} {user?.LastName} / {user?.UserName}"
                });
            }
        }

        return model;
    }
}