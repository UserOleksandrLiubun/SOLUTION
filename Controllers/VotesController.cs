using CHOICE.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
public class CreateVoteViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; }
    public bool IsPrivate { get; set; }
    public string Description { get; set; }

    [Display(Name = "Start Date")]
    public DateTime StartDateTime { get; set; } = DateTime.Now;

    [Display(Name = "End Date")]
    public DateTime EndDateTime { get; set; } = DateTime.Now.AddDays(7);

    [Display(Name = "Allowed Users")]
    public List<string> UsersIDs { get; set; }
    public List<string> Alternatives { get; set; } = new();
    public List<VoteCriteriaViewModel> Criteria { get; set; } = new();
    public List<SelectListItem> Contacts { get; set; } = new();
}

public class VoteCriteriaViewModel
{
    [Required]
    public string Title { get; set; }

    public string Description { get; set; }

    [Range(1, 100)]
    public double Importance { get; set; } = 100;

    public double MinValue { get; set; } = 0;
    public double StepValue { get; set; } = 0;
    public double MaxValue { get; set; } = 10;
}

public class VoteEvaluationViewModel
{
    public int VoteId { get; set; }
    public string VoteTitle { get; set; }
    public string Description { get; set; }
    public bool IsPrivate { get; set; }
    public List<DBVoteAlternative> Alternatives { get; set; } = new();
    public List<EvaluationCriteriaViewModel> Criteria { get; set; } = new();
}

public class EvaluationCriteriaViewModel
{
    public int SettingsId { get; set; }
    public int DBVoteAlternativeId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    [Range(0, 100)]
    public double ImportanceValue { get; set; }

    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double StepValue { get; set; }
    [Range(0, double.MaxValue)]
    public double Value { get; set; }
}
public class VoteResultViewModel
{
    public string Title { get; set; }
    public bool IsPrivate { get; set; }
    public DBVote DBVote { get; set; }
    public List<DBVoteItemSettings> DBVoteItemSettings { get; set; }
    public List<DBVoteItem> DBVoteItem { get; set; }
    public List<DBVoteAlternative> DBVoteAlternative { get; set; }
}
[Authorize]
public class VotesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<DBApplicationUser> _userManager;

    public VotesController(ApplicationDbContext context, UserManager<DBApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var votes = _context.DBVotes
            .Where(v => v.UserId == userId || v.UsersIDs.Contains(userId))
            .ToList();
        return View(votes);
    }
    public async Task<List<SelectListItem>> GetContacts()
    {
        var user = await _userManager.GetUserAsync(User);
        var contacts = await _context.Contacts.Where(item => (item.UserId == user.Id || item.ContactUserId == user.Id) && item.IsAccepted == true).ToListAsync();
        List<DBApplicationUser> users = new();
        foreach (var contact in contacts)
        {
            if (contact.UserId == user.Id)
            {
                users.Add(await _userManager.FindByIdAsync(contact.ContactUserId));
            }
            else if (contact.ContactUserId == user.Id)
            {
                users.Add(await _userManager.FindByIdAsync(contact.UserId));
            }
        }
        users.Add(user);
        List <SelectListItem> selects = new();
        foreach (var contactUser in users)
        {
            selects.Add(new SelectListItem
            {
                Value = contactUser.Id,
                Text = $"{contactUser.FirstName} {contactUser.LastName} ({contactUser.UserName})"
            });
        }
        return selects;
    }

    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        var model = new CreateVoteViewModel
        {
            Alternatives = new(),
            Criteria = new List<VoteCriteriaViewModel>
            {
                new VoteCriteriaViewModel()
            },
            Contacts = await GetContacts()
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVoteViewModel model)
    {
        model.Contacts = await GetContacts();
        double totalImportance = model.Criteria.Sum(c => c.Importance);
        ViewBag.WeightError = null;
        if (totalImportance != 100 && totalImportance != model.Criteria.Count() * 100) 
        {
            ViewBag.WeightError = "Assign weights so that the sum of all criteria equals 100% or the importance of each criterion equals 100.";
            return View(model); 
        }
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var vote = new DBVote
            {
                Title = model.Title,
                IsPrivate = model.IsPrivate,
                Description = model.Description,
                StartDateTime = model.StartDateTime,
                EndDateTime = model.EndDateTime,
                UserId = userId,
                UsersIDs = string.Join(",", model.UsersIDs)
            };

            _context.DBVotes.Add(vote);
            await _context.SaveChangesAsync();

            foreach (var criteria in model.Criteria)
            {
                var settings = new DBVoteItemSettings
                {
                    DBVoteId = vote.Id,
                    Title = criteria.Title,
                    Description = criteria.Description,
                    ImportanceValue = criteria.Importance,
                    StepValue = criteria.StepValue,
                    MinValue = criteria.MinValue,
                    MaxValue = criteria.MaxValue
                };
                _context.DBVoteItemSettings.Add(settings);
            }

            foreach (var alternative in model.Alternatives)
            {
                var settings = new DBVoteAlternative
                {
                    DBVoteId = vote.Id,
                    Title = alternative
                };
                _context.DBVoteAlternative.Add(settings);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    public IActionResult Evaluate(int id)
    {
        var vote = _context.DBVotes.Find(id);
        if (vote == null) return NotFound();

        var criteria = _context.DBVoteItemSettings
            .Where(s => s.DBVoteId == id)
            .ToList();

        var alternative = _context.DBVoteAlternative
            .Where(s => s.DBVoteId == id)
            .ToList();

        var model = new VoteEvaluationViewModel
        {
            VoteId = id,
            IsPrivate = vote.IsPrivate,
            Description = vote.Description,
            VoteTitle = vote.Title,
            Alternatives = alternative,
            Criteria = criteria.Select(c => new EvaluationCriteriaViewModel
            {
                SettingsId = c.Id,
                ImportanceValue = c.ImportanceValue,
                StepValue = c.StepValue,
                Title = c.Title,
                Description = c.Description,
                MinValue = c.MinValue,
                MaxValue = c.MaxValue
            }).ToList()
        };

        if (alternative.Count > 0)
        {
            model.Criteria = new();
            for (int j = 0; j < alternative.Count; j++)
            {
                for (int i = 0; i < criteria.Count; i++)
                {
                    model.Criteria.Add(new EvaluationCriteriaViewModel
                    {
                        DBVoteAlternativeId = alternative[j].Id,
                        ImportanceValue = criteria[i].ImportanceValue,
                        StepValue = criteria[i].StepValue,
                        SettingsId = criteria[i].Id,
                        Title = criteria[i].Title,
                        Description = criteria[i].Description,
                        MinValue = criteria[i].MinValue,
                        MaxValue = criteria[i].MaxValue
                    });
                }
            }
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Evaluate(VoteEvaluationViewModel model)
    {
        // Check if vote exists and is still active
        var vote = await _context.DBVotes.FindAsync(model.VoteId);
        if (vote == null)
        {
            ModelState.AddModelError("", "Vote not found.");
            return View(model);
        }

        if (DateTime.Now < vote.StartDateTime || DateTime.Now > vote.EndDateTime)
        {
            ModelState.AddModelError("", "This vote is not currently active.");
            return View(model);
        }

        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user is allowed to vote
            if (!string.IsNullOrEmpty(vote.UsersIDs) && !vote.UsersIDs.Split(',').Contains(userId))
            {
                ModelState.AddModelError("", "You are not authorized to vote in this evaluation.");
                return View(model);
            }

            var existingVotes = await _context.DBVoteItems
                .Where(v => v.DBVoteId == model.VoteId && v.UserId == userId).ToListAsync();
            if (existingVotes.Count() > 0)
            {
                _context.DBVoteItems.RemoveRange(existingVotes);
            }
            foreach (var criteria in model.Criteria)
            {
                var voteItem = new DBVoteItem
                {
                    DBVoteAlternativeId = criteria.DBVoteAlternativeId,
                    DBVoteId = model.VoteId,
                    DBVoteItemSettingsId = criteria.SettingsId,
                    ImportanceValue = criteria.ImportanceValue,
                    Value = criteria.Value,
                    UserId = userId
                };
                _context.DBVoteItems.Add(voteItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Results), new { id = model.VoteId });
        }

        // Repopulate the model if validation fails
        var voteDetails = _context.DBVotes.Find(model.VoteId);
        model.VoteTitle = voteDetails?.Title;
        return View(model);
    }

    public IActionResult Results(int id)
    {
        var vote = _context.DBVotes.FirstOrDefault((item) => item.Id == id);
        if (vote == null) return NotFound();

        var dBVoteItemSettings = _context.DBVoteItemSettings
            .Where(s => s.DBVoteId == vote.Id)
            .ToList();

        var alternatives = _context.DBVoteAlternative
            .Where(v => v.DBVoteId == vote.Id)
            .ToList();

        var votes = _context.DBVoteItems
            .Where(v => v.DBVoteId == vote.Id)
            .ToList();

        var result = new VoteResultViewModel()
        {
            Title = vote.Title,
            IsPrivate = vote.IsPrivate,
            DBVote = vote,
            DBVoteItemSettings = dBVoteItemSettings,
            DBVoteItem = votes,
            DBVoteAlternative = alternatives
        };

        return View(result);
    }

    // POST: Contacts/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var votes = await _context.DBVotes
            .Where(c => c.UserId == currentUser.Id && c.Id == id)
            .ToListAsync();

        _context.DBVotes.RemoveRange(votes);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Vote removed successfully!";
        return RedirectToAction("Index");
    }
}

public static class LinqExtensions
{
    public static double SafeAverage<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
    {
        if (source == null || !source.Any())
            return 0.0;

        return source.Average(selector);
    }
}