using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoutLibrary.Models;
using ScoutLibrary.Service;

namespace ScoutWeb.Pages;

public class IndexModel : PageModel
{
    private readonly ScoutService _scoutService;

    public IndexModel(ScoutService scoutService)
    {
        _scoutService = scoutService;
    }

    public List<Member> memberList = new List<Member>();
    public string? ErrorMessage { get; set; }
    
    public void OnGet()
    {
        try
        {
            memberList = _scoutService.ReadMember();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}