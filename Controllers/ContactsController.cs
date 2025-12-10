using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
[Authorize]
public class ContactsController : Controller
{
    private readonly UserManager<DBApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public ContactsController(UserManager<DBApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    public async Task<IActionResult> Search(UserSearchViewModel model)
    {
        var users = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(model.SearchTerm))
        {
            var searchTerm = model.SearchTerm.Trim().ToLower();

            users = users.Where(u =>
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.UserName.ToLower().Contains(searchTerm) ||
                u.Email.ToLower().Contains(searchTerm)
            );
        }

        model.Users = await users.OrderBy(u => u.LastName)
                                .ThenBy(u => u.FirstName)
                                .ToListAsync();

        return View(model);
    }
    // GET: Contacts/Add
    public IActionResult Add()
    {
        return View();
    }

    // POST: Contacts/Add
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddContactFromSearch(string userName)
    {
        return await AddCONTACT(userName, "Search");
    }

    // POST: Contacts/Add
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string userName)
    {
        return await AddCONTACT(userName, "Pending");
    }

    public async Task<IActionResult> AddCONTACT(string userName, string redirecntTo)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var contactUser = await _userManager.FindByNameAsync(userName);
        if (contactUser == null)
        {
            ModelState.AddModelError("", "User not found.");
            return View();
        }

        if (currentUser.Id == contactUser.Id)
        {
            ModelState.AddModelError("", "You cannot add yourself as a contact.");
            return View();
        }

        // Check if contact already exists in either direction
        bool contactExists = await _context.Contacts
            .AnyAsync(c => (c.UserId == currentUser.Id && c.ContactUserId == contactUser.Id) ||
                          (c.UserId == contactUser.Id && c.ContactUserId == currentUser.Id));

        if (contactExists)
        {
            ModelState.AddModelError("", "Contact relationship already exists.");
            return View();
        }

        var contact = new DBContact
        {
            UserId = currentUser.Id,
            ContactUserId = contactUser.Id,
            IsAccepted = false
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Запит на контакт успішно надіслано!";
        return RedirectToAction(redirecntTo);
    }

    // POST: Contacts/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string userName)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var contactUser = await _userManager.FindByNameAsync(userName);
        if (contactUser == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Index");
        }

        var contacts = await _context.Contacts
            .Where(c => (c.UserId == currentUser.Id && c.ContactUserId == contactUser.Id) ||
                       (c.UserId == contactUser.Id && c.ContactUserId == currentUser.Id))
            .ToListAsync();

        if (!contacts.Any())
        {
            TempData["Error"] = "Contact not found.";
            return RedirectToAction("Index");
        }

        _context.Contacts.RemoveRange(contacts);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Контакт успішно видалено!";
        return RedirectToAction("Index");
    }

    // POST: Contacts/Accept
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(string userName)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var contactUser = await _userManager.FindByNameAsync(userName);
        if (contactUser == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Pending");
        }

        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.UserId == contactUser.Id &&
                                     c.ContactUserId == currentUser.Id &&
                                     !c.IsAccepted);

        if (contact == null)
        {
            TempData["Error"] = "Contact request not found.";
            return RedirectToAction("Pending");
        }

        contact.IsAccepted = true;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Контакт додано!";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Pending()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        // Get pending requests where current user is the recipient
        var pendingRequests = await _context.Contacts
            .Where(c => c.ContactUserId == currentUser.Id && !c.IsAccepted)
            .ToListAsync();

        // Get usernames for each pending request
        var pendingViewModels = new List<PendingContactViewModel>();
        foreach (var request in pendingRequests)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user != null)
            {
                pendingViewModels.Add(new PendingContactViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    RequestId = request.Id
                });
            }
        }

        return View(pendingViewModels);
    }

    // GET: Contacts/Index
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        // Get all accepted contacts where current user is either UserId or ContactUserId
        var contacts = await _context.Contacts
            .Where(c => (c.UserId == currentUser.Id || c.ContactUserId == currentUser.Id) && c.IsAccepted)
            .ToListAsync();

        var contactViewModels = new List<ContactViewModel>();
        foreach (var contact in contacts)
        {
            string otherUserId;
            bool isInitiator;

            if (contact.UserId == currentUser.Id)
            {
                otherUserId = contact.ContactUserId;
                isInitiator = true;
            }
            else
            {
                otherUserId = contact.UserId;
                isInitiator = false;
            }

            var otherUser = await _userManager.FindByIdAsync(otherUserId);
            if (otherUser != null)
            {
                contactViewModels.Add(new ContactViewModel
                {
                    UserName = otherUser.UserName,
                    IsInitiator = isInitiator
                });
            }
        }

        return View(contactViewModels);
    }
}

// ViewModels for display
public class ContactViewModel
{
    public string UserName { get; set; }
    public bool IsInitiator { get; set; }
}

public class PendingContactViewModel
{
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int RequestId { get; set; }
}