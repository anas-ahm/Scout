using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Asn1.Mozilla;
using ScoutLibrary.Models;
using ScoutLibrary.Repositories;
using System.Security.Claims;
using ScoutLibrary.Service;

namespace ScoutWeb.Pages;

public class IndexModel : PageModel
{
    // Connecting to Servivce
    private readonly ScoutService _scoutService;
    private readonly ActivityService _activityService;
    private readonly ICRUD<Member> _memberRepository;

    // Parameters to be used on page
    public List<Member> memberList = new List<Member>();
    public List<Activity> AllActivities = new List<Activity>();
    public string? ErrorMessage { get; set; }

    // Member object to hold the currently logged-in member's information
    public Member? LoggedInMember { get; set; }
    
    // List for storing the activity id's the logged in member is signed up for.
    public List<int?> AssignedActivityIds { get; private set; } = new();
    
    // ILogger used to log exceptions in production code for us (devs) inside terminal
    // Users only see a generic error code
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ScoutService scoutService, ActivityService activityService, ICRUD<Member> memberRepository, ILogger<IndexModel> logger)
    {
        _scoutService = scoutService;
        _activityService = activityService;
        _memberRepository = memberRepository;
        _logger = logger;
    }

    // Runs when page is loaded
    public void OnGet()
    {
        // moved to a void method
        LoadLoggedInMember();

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
            TempData["ErrorMessage"] = "Data kunne ikke hentes. Prøv igen senere.";
        }
        
        return Page();
    }
    
    public IActionResult OnPostDeleteActivity(int id)
    {
        try
        {
            _activityService.Delete(id);
            TempData["SuccessMessage"] = "Aktiviteten blev slettet succesfuldt.";  //tempdata gemmes i én refresh
            OnGet();
            return RedirectToPage(); // Genindlæser siden
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Kunne ikke slette aktiviteten.");
            return Page();
        }
    }

   
    
    
    // OnPost for signing up for an activity
    public IActionResult OnPostAssignActivity(int activityId)
    {
        try
        {
            LoadLoggedInMember();

            //adding activity to LoggedInMember 
            LoggedInMember.Activities.Add(_activityService.GetById(activityId));
          
            _scoutService.AssignActivityToMember(LoggedInMember.MemberId, activityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't assign activity to member");
            TempData["ErrorMessage"] = "Kan ikke tilføje aktivitet, Prøv igen senere.";
            
        }

        TempData["SuccessMessage"] = "Du er nu tilmeldt!";
        return RedirectToPage();
    }
    
    
    // OnPost for Unassigning up for an activity
    public IActionResult OnPostUnAssignActivity(int activityId)
    {
        try
        {
            LoadLoggedInMember();

            //removing activity to LoggedInMember 
            LoggedInMember.Activities.Remove(_activityService.GetById(activityId));
          
            _scoutService.UnAssignActivityToMember(LoggedInMember.MemberId, activityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't assign activity to member");
            TempData["ErrorMessage"] = "Kan ikke fjerne aktivitet, Prøv igen senere.";
            
        }

        TempData["SuccessMessage"] = "Du er nu afmeldt!";
        return RedirectToPage();
    }

    // Loads the logged-in member and the assigned activity id's - 
    private void LoadLoggedInMember()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            int id = int.Parse(userId);

            LoggedInMember = _memberRepository.GetById(id);

            // LINQ Select that goes through all the logged in members Activities, selects the id's
            // and puts them into AssignedActivityIds
            AssignedActivityIds = LoggedInMember.Activities.Select(x => x.ActivityId).ToList();        
        }
    }
    
}