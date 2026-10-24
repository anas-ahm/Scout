using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoutLibrary.Models;
using ScoutLibrary.Service;

namespace ScoutWeb.Pages.crud;

public class ActivityDetailsModel : PageModel
{
    private readonly ActivityService _activityService;
    public Activity activity = new Activity();

    public ActivityDetailsModel(ActivityService activityService)
    {
        _activityService = activityService;
    }
    public IActionResult OnGet(int id)
    {
        if (_activityService.GetById(id) == null)
        {
            TempData["ErrorMessage"] = "Aktivitet ikke fundet - prøv igen";
            return RedirectToPage("Index");
        }
        activity = _activityService.GetById(id);
        return Page();
    }
}