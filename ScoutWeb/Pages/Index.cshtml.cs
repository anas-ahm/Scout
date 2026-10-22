using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoutLibrary.Models;
using ScoutLibrary.Service;

namespace ScoutWeb.Pages;

public class IndexModel : PageModel
{
    // Connecting to Servivce
    private readonly ScoutService _scoutService;

    public IndexModel(ScoutService scoutService)
    {
        _scoutService = scoutService;
    }

    // Parameters to be used on page
    public List<Member> memberList = new List<Member>();
    public string? ErrorMessage { get; set; }
    public int memberToDelete { get; set; }
    

    // OnPost for when clicking delete first time
    public void OnPostAskDeletion(int id)
    {
        memberToDelete = id;

        try
        {
            memberList = _scoutService.ReadAllMembers();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
    
    // OnPost for viewing a members memberDetails page
    public void OnPostSeeMemberDetails(int id)
    {
        
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
            ErrorMessage = "Spejderen findes ikke længere.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        // Run same ReadAllMembers from OnGet, since page gets reloaded 
        // and the OnGet wont run again
        try
        {
            memberList = _scoutService.ReadAllMembers();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }
    
    // Runs when page is loaded
    public void OnGet()
    {
        try
        {
            memberList = _scoutService.ReadAllMembers();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}