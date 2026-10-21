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
        private readonly ICRUD<Member> _memberRepository;

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
        public List<int>? RequiredBadges { get; set; }  //gemmes som int, da man ikke kan bruge hele objekter i checkbokse
        [BindProperty]
        public List<int> Members { get; set; } //se ovenfor
        

        // Denne liste bruges til at vise alle eksisterende badges og ledere i HTML-formularen
        public List<Badge> AvailableBadges { get; set; } = new List<Badge>();        
        public List<Member> AvailableLeaders { get; set; } = new();
        


        public CreateActivityModel(ICRUD<Activity> activityRepository, ICRUD<Badge> badgeRepository, ICRUD<Member> memberRepository)
        {
            _activityRepository = activityRepository;
            _badgeRepository = badgeRepository;
            _memberRepository = memberRepository;
        }

        public void OnGet()
        {
            AvailableBadges = _badgeRepository.GetAll();
            AvailableLeaders = _memberRepository.GetAll().Where(leader => leader.IsLeader).ToList();
        }


        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }

            //ID'er skal omdannes til objekter inden de gemmes i Activity objektet.
            List<Badge> selectedBadges = AvailableBadges.Where(b => RequiredBadges.Contains(b.BadgeId)).ToList();
            List<Member> selectedMembers = AvailableLeaders.Where(m=>Members.Contains(m.MemberId)).ToList();

            Activity activity = new Activity
            {
                Name = Name,
                DateTime = DateTime,
                Description = Description,
                MaxCount = MaxCount,
                MinAge = MinAge,
                RequiredBadges = selectedBadges,
                Members = selectedMembers                      
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
