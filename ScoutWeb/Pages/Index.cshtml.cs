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
        if (User.Identity?.IsAuthenticated == true)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            int id = int.Parse(userId);

            LoggedInMember = _memberRepository.GetById(id);
        }

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

   
    
}