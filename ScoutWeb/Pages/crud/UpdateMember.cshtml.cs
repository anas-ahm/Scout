using Microsoft.AspNetCore.Mvc;
using ScoutLibrary.Models;
using ScoutLibrary.Service;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;



namespace ScoutWeb.Pages.crud
{
    public class UpdateMemberModel : PageModel
    {
        // Connnecting to service
        private readonly ScoutService _scoutService;

        public UpdateMemberModel(ScoutService scoutService)
        {
            _scoutService = scoutService;
        }
        
        public Member? memberToUpdate { get; set; }
        public bool memberFound { get; set; }
        
        [BindProperty] 
        public InputModel Input { get; set; } = new();
        
        
        public IActionResult OnGet(int id)
        {
            memberToUpdate = _scoutService.GetById(id);

            if (memberToUpdate == null)
            {
                TempData["ErrorMessage"] = "Spejderen blev ikke fundet!";
                return RedirectToPage("/crud/MemberDetails");
            }
            
            memberFound = true;
            return Page();
        }

        public IActionResult OnPostUpdateMember(int id)
        {
            // Only check digits if a value was actually provided ([Required] reports the missing case)
            if (!string.IsNullOrEmpty(Input.PhoneNumber) && !Input.PhoneNumber.All(char.IsDigit))
            {
                ModelState.AddModelError("Input.PhoneNumber", "Telefonnummer må kun indeholde tal");
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
            memberToCreate.FirstName = Input.FirstName;
            memberToCreate.LastName = Input.LastName;
            memberToCreate.DateOfBirth = Input.DateOfBirth;
            memberToCreate.PhoneNumber = Input.PhoneNumber;
            memberToCreate.IsLeader = Input.IsLeader;
            memberToCreate.Username = Input.Username;
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

            TempData["SuccessMessage"] = $"{Input.FirstName} {Input.LastName} blev opdateret";
            
            return Redirect($"/crud/MemberDetails/{id}");
        }

        
        public class InputModel
        {
            [Required(ErrorMessage = "Fornavn mangler")]
            public string FirstName { get; set; } = "";

            [Required(ErrorMessage = "Efternavn mangler")]
            public string LastName { get; set; } = "";
            
            [Required(ErrorMessage = "Fødselsdato mangler")]
            public DateTime DateOfBirth { get; set; }

            [Required(ErrorMessage = "Telefonnummer mangler")]
            [StringLength(8, MinimumLength = 8, ErrorMessage = "Telefonnummer skal være 8 tegn")]
            public string PhoneNumber { get; set; } = "";

            [Required(ErrorMessage = "Brugernavn mangler")]
            public string Username { get; set; } = "";
            public bool IsLeader { get; set; }
        }
    }
}
