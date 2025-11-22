using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
public class CreateVoteViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    public string Description { get; set; }

    [Display(Name = "Start Date")]
    public DateTime StartDateTime { get; set; } = DateTime.Now;

    [Display(Name = "End Date")]
    public DateTime EndDateTime { get; set; } = DateTime.Now.AddDays(7);

    [Display(Name = "Allowed User IDs (comma-separated)")]
    public string UsersIDs { get; set; }
    public List<string> Alternatives { get; set; } = new();
    public List<VoteCriteriaViewModel> Criteria { get; set; } = new();
    public List<Contact> Contacts { get; set; } = new();
}

public class VoteCriteriaViewModel
{
    [Required]
    public string Title { get; set; }

    public string Description { get; set; }

    [Range(1, 100)]
    public double MaxImportance { get; set; } = 100;

    public double MinValue { get; set; } = 0;

    public double MaxValue { get; set; } = 10;
}

public class VoteEvaluationViewModel
{
    public int VoteId { get; set; }
    public string VoteTitle { get; set; }
    public List<EvaluationCriteriaViewModel> Criteria { get; set; } = new();
}

public class EvaluationCriteriaViewModel
{
    public int SettingsId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    [Range(0, 100)]
    public double ImportanceValue { get; set; }

    public double MinValue { get; set; }
    public double MaxValue { get; set; }

    [Range(0, double.MaxValue)]
    public double Value { get; set; }
}
[Authorize]
public class VotesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public VotesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        var model = new CreateVoteViewModel
        {
            Criteria = new List<VoteCriteriaViewModel>
            {
                new VoteCriteriaViewModel()
            },
            Contacts = _context.Contacts.Where((item) => item.UserId.Contains(user.Id) || item.ContactUserId.Contains(user.Id)).ToList()
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVoteViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var vote = new DBVote
            {
                Title = model.Title,
                Description = model.Description,
                StartDateTime = model.StartDateTime,
                EndDateTime = model.EndDateTime,
                UserId = userId,
                UsersIDs = model.UsersIDs
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
                    MaxImportanceValue = criteria.MaxImportance,
                    MinValue = criteria.MinValue,
                    MaxValue = criteria.MaxValue
                };
                _context.DBVoteItemSettings.Add(settings);
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

        var model = new VoteEvaluationViewModel
        {
            VoteId = id,
            VoteTitle = vote.Title,
            Criteria = criteria.Select(c => new EvaluationCriteriaViewModel
            {
                SettingsId = c.Id,
                Title = c.Title,
                Description = c.Description,
                MinValue = c.MinValue,
                MaxValue = c.MaxValue
            }).ToList()
        };

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

            // Check if user already voted
            var existingVote = _context.DBVoteItems
                .FirstOrDefault(v => v.DBVoteId == model.VoteId && v.UserId == userId);

            if (existingVote != null)
            {
                ModelState.AddModelError("", "You have already voted for this evaluation.");
                return View(model);
            }

            foreach (var criteria in model.Criteria)
            {
                var voteItem = new DBVoteItem
                {
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
        var vote = _context.DBVotes.Find(id);
        if (vote == null) return NotFound();

        var criteria = _context.DBVoteItemSettings
            .Where(s => s.DBVoteId == id)
            .ToList();

        var votes = _context.DBVoteItems
            .Where(v => v.DBVoteId == id)
            .ToList();

        var results = criteria.Select(c => new
        {
            Criteria = c,
            AverageImportance = votes.Where(v => v.DBVoteItemSettingsId == c.Id)
                                   .SafeAverage(v => v.ImportanceValue),
            AverageValue = votes.Where(v => v.DBVoteItemSettingsId == c.Id)
                              .SafeAverage(v => v.Value),
            WeightedScore = votes.Where(v => v.DBVoteItemSettingsId == c.Id)
                               .SafeAverage(v => v.ImportanceValue * v.Value)
        }).ToList();

        ViewBag.VoteTitle = vote.Title;
        ViewBag.TotalVotes = votes.Select(v => v.UserId).Distinct().Count();
        return View(results);
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