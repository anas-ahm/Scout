using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoutLibrary.Repositories;
using ScoutLibrary.Models;
using Activity = ScoutLibrary.Models.Activity;

namespace ScoutWeb.Pages.crud
{
    public class CreateActivityModel : PageModel
    {
        private readonly ICRUD<Activity> _activityRepository;
        private readonly ICRUD<Badge> _badgeRepository;

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
        [BindProperty]
        public List<Badge>? RequiredBadges { get; set; }

        // Denne liste bruges til at vise alle tilgængelige badges i HTML-formularen
        public List<Badge> AvailableBadges { get; set; } = new List<Badge>();


        public CreateActivityModel(ICRUD<Activity> activityRepository, ICRUD<Badge> badgeRepository)
        {
            _activityRepository = activityRepository;
            _badgeRepository = badgeRepository;            
        }

        public void OnGet()
        {
            AvailableBadges = _badgeRepository.GetAll();
        }


        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }

            Activity activity = new Activity 
            { 
                Name = Name, 
                DateTime = DateTime, 
                Description = Description, 
                MaxCount = MaxCount, 
                MinAge = MinAge, 
                RequiredBadges = RequiredBadges, 
                Members = null                          //kan evt automatisk tilføje den leder, der oprettede aktiviteten
            };

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
