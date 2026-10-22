using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoutLibrary.Models;
using ScoutLibrary.Service;

namespace ScoutWeb.Pages;

public class IndexModel : PageModel
{
    // Connecting to Servivce
    private readonly ScoutService _scoutService;
    private readonly ActivityService _activityService;
    
    // ILogger used to log exceptions in production code for us (devs) inside terminal
    // Users only see a generic error code
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ScoutService scoutService, ActivityService activityService, ILogger<IndexModel> logger)
    {
        _scoutService = scoutService;
        _activityService = activityService;
        _logger = logger;
    }

    // Parameters to be used on page
    public List<Member> memberList = new List<Member>();
    public List<Activity> AllActivities = new List<Activity>();
    public string? ErrorMessage { get; set; }

    // Runs when page is loaded
    public void OnGet()
    {
        try
        {
            memberList = _scoutService.ReadAllMembers();
            AllActivities = _activityService.GetAll();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read all members!");
            ErrorMessage = "Noget gik galt ved at indhente alle spejdere, prøv igen senere";
        }
    }
    
    // OnPost for cofirmation of deletion
    public IActionResult OnPostConfirmMemberDeletion(int id)
    {
        try
        {
            _scoutService.DeleteMember(id);
            TempData["SuccessMessage"] = "Spejderen blev slettet";
            return RedirectToPage();
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Tried to delete member {MemberId}, but it was not found", id);
            ErrorMessage = "Spejderen findes ikke længere.";
        }

        // Run same ReadAllMembers from OnGet, since page gets reloaded 
        // and the OnGet wont run again
        try
        {
            memberList = _scoutService.ReadAllMembers();
            AllActivities = _activityService.GetAll();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read all members!");
            
            // "??" after ErrorMessage means "only assign if ErrorMessage is null"
            ErrorMessage ??= "Data kunne ikke hentes. Prøv igen senere.";
        }
        
        return Page();
    }
    
    
}