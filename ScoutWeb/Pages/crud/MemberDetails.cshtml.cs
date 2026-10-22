using Microsoft.AspNetCore.Mvc;
using ScoutLibrary.Models;
using ScoutLibrary.Service;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ScoutWeb.Pages.crud;

public class MemberDetailsModel : PageModel
{
    private readonly ScoutService _scoutService;

    public MemberDetailsModel(ScoutService scoutService)
    {
        _scoutService = scoutService;
    }
    
    
    public Member? memberToShow { get; set; }
    public bool memberFound { get; set; }
    
    
    public IActionResult OnGet(int id)
    {
        memberToShow = _scoutService.GetById(id);

        if (memberToShow == null)
        {
            TempData["ErrorMessage"] = "Spejderen blev ikke fundet!";
            return RedirectToPage("/Index");
        }

        memberFound = true;
        return Page();
    }
    
    
}