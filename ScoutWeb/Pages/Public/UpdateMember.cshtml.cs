using Microsoft.AspNetCore.Mvc;
using ScoutLibrary.Models;
using ScoutLibrary.Service;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ScoutLibrary.Repositories;


namespace ScoutWeb.Pages.crud
{
    public class UpdateMemberModel : PageModel
    {
        // Connnecting to service
        private readonly ScoutService _scoutService;
        private readonly ICRUD<Badge> _badgeService;
        private readonly ICRUD<Member> _memberRepository;

        public UpdateMemberModel(ScoutService scoutService, ICRUD<Member> memberRepository, ICRUD<Badge> badgeService)
        {
            _scoutService = scoutService;
            _memberRepository = memberRepository;
            _badgeService = badgeService;
        }
        
        public Member? memberToUpdate { get; set; }
        public bool memberFound { get; set; }
        public List<Badge> allBadges { get; set; } = new();
        
        public LeaderInputModel LeaderInput { get; set; } = new();
        public ScoutInputModel ScoutInput { get; set; } = new();
        
        
        public IActionResult OnGet(int id)
        {
            allBadges = _badgeService.GetAll();  
            
            memberToUpdate = _scoutService.GetById(id);

            if (memberToUpdate == null)
            {
                TempData["ErrorMessage"] = "Spejderen blev ikke fundet!";
                return RedirectToPage("/Public/MemberDetails");
            }
            
            ScoutInput = new ScoutInputModel
            {
                PhoneNumber = memberToUpdate.PhoneNumber
            };

            memberFound = true;
            return Page();
        }

        public IActionResult OnPostLeaderUpdateMember(int id, [Bind(Prefix = nameof(LeaderInput))] LeaderInputModel input)
        {
            if (!User.IsInRole("Leader"))
            {
                return Forbid();
            }
            
            allBadges = _badgeService.GetAll();  
            memberToUpdate = _scoutService.GetById(id);
            
            if (memberToUpdate == null)
            {
                TempData["ErrorMessage"] = "Spejderen blev ikke fundet!";
                return RedirectToPage("/Index");
            }
            
            // Now this sees the error above as well as the attribute errors
            if (!ModelState.IsValid)
            {
                // get member again before return call, so its not null and server crashes
                memberToUpdate = _scoutService.GetById(id);
                return Page();
            }
            
            // Building the object and pass it though the repo
            Member memberToCreate = new Member();
            memberToCreate.MemberId = id;
            memberToCreate.FirstName = input.FirstName;
            memberToCreate.LastName = input.LastName;
            memberToCreate.DateOfBirth = input.DateOfBirth;
            memberToCreate.PhoneNumber = input.PhoneNumber;
            memberToCreate.IsLeader = input.IsLeader;
            memberToCreate.Username = input.Username;
            
            // Only keep the badges that were selected in the dropdown
            memberToCreate.Badges = allBadges
                .Where(b => input.SelectedBadgeIds.Contains(b.BadgeId))
                .ToList();
            
            try
            {
                _scoutService.Update(memberToCreate, id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                
                // get member again before return call, so its not null and server crashes
                memberToUpdate = _scoutService.GetById(id);
                return Page();
            }

            TempData["SuccessMessage"] = $"{input.FirstName} {input.LastName} blev opdateret";
            
            return Redirect($"/medlem/{id}");
            
        }

        public IActionResult OnPostScoutUpdateMember(int id, [Bind(Prefix = nameof(ScoutInput))] ScoutInputModel input)
        {
            // memberToUpdate is loaded again after OnPost Call
            memberToUpdate = _scoutService.GetById(id);
            if (memberToUpdate == null)
            {
                TempData["ErrorMessage"] = "Spejderen blev ikke fundet!";
                return RedirectToPage("/Index");
            }
            
            // Now this sees the error above as well as the attribute errors
            if (!ModelState.IsValid)
            {
                // get member again before return call, so its not null and server crashes
                memberToUpdate = _scoutService.GetById(id);
                return Page();
            }
            
            
            // Building the object and pass it though the repo
            Member memberToCreate = new Member();
            memberToCreate.MemberId = id;
            memberToCreate.FirstName = memberToUpdate.FirstName;
            memberToCreate.LastName = memberToUpdate.LastName;
            memberToCreate.DateOfBirth = memberToUpdate.DateOfBirth;
            memberToCreate.PhoneNumber = input.PhoneNumber;
            memberToCreate.IsLeader = memberToUpdate.IsLeader;
            memberToCreate.Username = memberToUpdate.Username;
            
            memberToCreate.Badges = memberToUpdate.Badges;
            
            try
            {
                _scoutService.Update(memberToCreate, id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                
                // get member again before return call, so its not null and server crashes
                memberToUpdate = _scoutService.GetById(id);
                return Page();
            }

            TempData["SuccessMessage"] = $"{memberToUpdate.FirstName} {memberToUpdate.LastName} blev opdateret";
            
            return Redirect($"/medlem/{id}");
        }

        
        public class ScoutInputModel
        {
            
            [Required(ErrorMessage = "Telefonnummer mangler")]
            [RegularExpression(@"^\d{8}$", ErrorMessage = "Telefonnummer skal være præcis 8 tal")]
            public string PhoneNumber { get; set; } = "";
         
        }
        
        public class LeaderInputModel
        {
            [Required(ErrorMessage = "Fornavn mangler")]
            public string FirstName { get; set; } = "";

            [Required(ErrorMessage = "Efternavn mangler")]
            public string LastName { get; set; } = "";
            
            [Required(ErrorMessage = "Fødselsdato mangler")]
            public DateTime DateOfBirth { get; set; }
            
            [Required(ErrorMessage = "Telefonnummer mangler")]
            [RegularExpression(@"^\d{8}$", ErrorMessage = "Telefonnummer skal være præcis 8 tal")]
            public string PhoneNumber { get; set; } = "";
            
            [Required(ErrorMessage = "Brugernavn mangler")]
            public string Username { get; set; } = "";
            public bool IsLeader { get; set; }
            public List<int> SelectedBadgeIds { get; set; } = new();
        }
    }
}
