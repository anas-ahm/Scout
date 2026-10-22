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
    public void OnGet(int id)
    {
        activity = _activityService.GetById(id);
    }
}