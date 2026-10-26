using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoutLibrary.Models;
using ScoutLibrary.Repositories;
using ScoutLibrary.Service;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using Activity = ScoutLibrary.Models.Activity;

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

        //Til form select multiple skal bruges en liste, men den kan IKKE holde på hele objekter. Derfor bruges int ell. string.
        [BindProperty]
        public List<int>? SelectedBadges { get; set; } = new List<int>();

        [BindProperty]
        [Required(ErrorMessage = "Du skal vælge mindst én leder til aktiviteten.")] //denne del sikrer at listen ikke må være null
        [MinLength(1, ErrorMessage = "Du skal vælge mindst én leder til aktiviteten.")] //denne del sikrer at .count er minimum 1 
        public List<int> SelectedLeaders { get; set; } = new List<int>();


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
            AvailableLeaders = _memberService.ReadAllMembers().Where(m => m.IsLeader).ToList();
        }


        public IActionResult OnPost()
        {
            //Betingelse kan skrives her ELLER under [bindproperty], som jeg har gjort. Efterladt for at vise det.
            //if (SelectedLeaders == null || SelectedLeaders.Count == 0)
            //{
            //    ModelState.AddModelError("SelectedLeaders", "Du skal vælge mindst én leder til aktiviteten.");
            //}


            // Håndterer brugerfejl (validering)
            if (!ModelState.IsValid)
            {
                OnGet();  //Henter listerne igen så formularen ikke crasher.
                return Page();
            }

            //når onpost kører resettes listerne vi fyldte i OnGet, derfor fylder vi listerne igen
            var allBadges = _badgeService.GetAll();
            var allLeaders = _memberService.ReadAllMembers().Where(m => m.IsLeader).ToList();


            //SelectedIds, som er bundet via BindProperty, bruges her til at lave en liste af objekterne:
            var requiredBadges = allBadges
                                .Where(b => SelectedBadges?.Contains(b.BadgeId) == true)
                                .ToList();

            var members = allLeaders
                        .Where(l => SelectedLeaders?.Contains(l.MemberId) == true)
                        .ToList();


            Activity activity = new Activity
            {
                Name = Name,
                DateTime = DateTime,
                Description = Description,
                MaxCount = MaxCount,
                MinAge = MinAge,
                RequiredBadges = requiredBadges,
                Members = members
            };

            try
            {
                _activityService.Create(activity);
                TempData["SuccessMessage"] = "Aktivitet oprettet succesfuldt!";
                return RedirectToPage("/Index");
            }
            catch(Exception ex) //Håndterer tekniske systemfejl, kræver at der kastes en exception, da det ikke kan løses af brugeren.
            {
                ModelState.AddModelError(string.Empty, "Kunne ikke gemme aktiviteten. Prøv venligst igen eller kontakt systemadministrator.");
                OnGet();
                return Page();
            }
        }
    }
}
