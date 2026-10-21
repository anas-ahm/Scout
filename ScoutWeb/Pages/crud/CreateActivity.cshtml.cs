using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoutLibrary.Repositories;
using Activity = ScoutLibrary.Models.Activity;

namespace ScoutWeb.Pages.crud
{
    public class CreateActivityModel : PageModel
    {
        private readonly ICRUD<Activity> _activityRepository;

        [BindProperty]
        public string Name { get; set; }
        [BindProperty]
        public DateTime DateTime { get; set; }
        [BindProperty]
        public string? Description { get; set; }
        [BindProperty]
        public int? MaxCount { get; set; }
        [BindProperty]
        public int? MinAge { get; set; }

        

        
        public CreateActivityModel(ICRUD<Activity> activityRepository)
        {
            _activityRepository = activityRepository;            
        }

        public void OnGet()
        {
        }


        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }
            Activity activity = new Activity { Name = Name, DateTime = DateTime, Description = Description, MaxCount = MaxCount, MinAge = MinAge, RequiredBadges = null, Members = null };
            try
            {
                _activityRepository.Create(activity);
                TempData["SuccessMessage"] = "Aktivitet oprettet succesfuldt!";
                return RedirectToPage("/Index");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Kunne ikke gemme medarbejderen. Prøv venligst igen eller kontakt systemadministrator.");
                throw new Exception("Fejl ved oprettelse af medarbejder i databasen", ex);
            }
        }
    }
}
