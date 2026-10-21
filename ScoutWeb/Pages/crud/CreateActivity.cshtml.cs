using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoutLibrary.Repositories;
using ScoutLibrary.Models;
using Activity = ScoutLibrary.Models.Activity;
using ScoutLibrary.Service;
using System.ComponentModel.Design.Serialization;

namespace ScoutWeb.Pages.crud
{
    public class CreateActivityModel : PageModel
    {
        private readonly ActivityService _activityService;
        private readonly BadgeService _badgeService;
        private readonly ScoutService _memberService;

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
        public List<int>? SelectedBadges { get; set; }
        [BindProperty]
        public List<int>? SelectedLeaders { get; set; } //skal ikke tillade null når getall er lavet.

        // Denne liste bruges til at vise alle eksisterende badges og ledere i HTML-formularen
        public List<Badge>? AvailableBadges { get; set; } = new List<Badge>();
        public List<Member>? AvailableLeaders { get; set; } = new List<Member>();


        public CreateActivityModel(ActivityService activityService, BadgeService badgeService, ScoutService memberService)
        {
            _activityService = activityService;
            _badgeService = badgeService;
            _memberService = memberService;
        }

        public void OnGet()
        {
            AvailableBadges = _badgeService.GetAll();
            //AvailableLeaders = _memberService.GetAll().Where(m => m.IsLeader).ToList();
        }


        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }

            //når onpost kører resettes listerne vi fyldte i OnGet, derfor henter vi igen
            var allBadges = _badgeService.GetAll();
            //var allLeaders = _memberService.GetAll().Where(m => m.IsLeader).ToList();

            //Listen af valgte id'er bundet via BindModel skal laves om til en liste af objekterne:
            var requiredBadges = allBadges
                                .Where(b => SelectedBadges?.Contains(b.BadgeId) == true)
                                .ToList();

            //var members = allLeaders
            //            .Where(l => SelectedLeaders?.Contains(l.MemberId) == true)
            //            .ToList();
            

            Activity activity = new Activity
            {
                Name = Name,
                DateTime = DateTime,
                Description = Description,
                MaxCount = MaxCount,
                MinAge = MinAge,
                RequiredBadges = requiredBadges,
                Members = null
            };

            try
            {
                _activityService.Create(activity);
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
