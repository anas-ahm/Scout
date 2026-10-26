using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ScoutLibrary.Models;
using ScoutLibrary.Service;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using System.Security.Permissions;

namespace ScoutWeb.Pages.crud
{
    public class UpdateActivityModel : PageModel
    {
        private readonly ActivityService _activityService;
        private readonly BadgeService _badgeService;
        private readonly ScoutService _scoutService;
     

        public List<Badge> allBadges = new List<Badge>();           //for viewing and selecting
        public List<Member> allMembers = new List<Member>();
        public Activity Activity { get; set; }

        //to bind form input
        [BindProperty]
        public int ActivityId { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Navn skal udfyldes.")]
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
        [Required(ErrorMessage ="Der skal vælges mindst en leder til en aktivitet.")]
        [MinLength(1,ErrorMessage ="Der skal vælges mindst en leder til en aktivitet.")]
        public List<int> SelectedMembersId { get; set; } = new List<int>();         //binds id as int to a list
        [BindProperty]
        public List<int>? SelectedBadgesId { get; set; } = new List<int>();


       

        public UpdateActivityModel(ActivityService a, BadgeService b, ScoutService s)
        {
            _activityService = a;
            _badgeService = b;
            _scoutService = s;
          
        }

        public void OnGet(int id)
        {
            LoadDropdowns(); //populerer listerne

            Activity = _activityService.GetById(id);

            if (Activity != null)
            {
                ActivityId = id; 
                Name = Activity.Name;
                DateTime = Activity.DateTime;
                MaxCount = Activity.MaxCount;
                MinAge = Activity.MinAge;
                Description = Activity.Description;

                // til at pre-vælge listerne i formularen, når siden loades første gang:
                SelectedMembersId = Activity.Members?.Select(m => m.MemberId).ToList() ?? new List<int>();
                SelectedBadgesId = Activity.RequiredBadges?.Select(b => b.BadgeId).ToList() ?? new List<int>();
            }
        }

        public IActionResult OnPost()
        {
            LoadDropdowns(); //repopulerer listerne af objekter

            List<Member> selectedLeaders = allMembers               //udvælger ledere fra den liste af ID'er der er valgt
                .Where(m =>SelectedMembersId.Contains(m.MemberId))
                .Where(m=>m.IsLeader).ToList();

            if (selectedLeaders.Count == 0) //checker om listen er tom
            {
                ModelState.AddModelError(String.Empty,"Der skal vælges mindst én leder, for at gemme aktiviteten");
            }

            if (!ModelState.IsValid)        //kun hvis validering består fortsætter vi
            {
                LoadDropdowns();                
                return Page();
            }

            
                        
            Activity updatedActivity = new Activity
            {
                ActivityId = ActivityId,
                Name = Name,
                DateTime = DateTime,
                Description = Description,
                MaxCount = MaxCount,
                MinAge = MinAge,
                //via LINQ filtereres lister med objekter og afleverer de objekter der indeholder SelectedIds
                Members = allMembers.Where(m => SelectedMembersId.Contains(m.MemberId)).ToList(),  
                RequiredBadges = allBadges.Where(b => SelectedBadgesId.Contains(b.BadgeId)).ToList()
            };
            

            try
            {
                _activityService.Update(updatedActivity, ActivityId);
                TempData["SuccessMessage"] = "Aktiviteten blev opdateret succesfuldt";
                return RedirectToPage("../Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Fejl ved opdatering i databasen. Prøv igen senere.");
                LoadDropdowns();
                return Page();
            }
        }

        private void LoadDropdowns()
        {
            allBadges = _badgeService.GetAll();
            allMembers = _scoutService.ReadAllMembers();
        }
    }
}
