using Microsoft.AspNetCore.Mvc;
using ScoutLibrary.Repositories;
using ScoutLibrary.Models;
using ScoutLibrary.Service;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ScoutWeb.Pages.crud
{
    public class CreateMemberModel : PageModel
    {
        private readonly ScoutService _scoutService;

        public CreateMemberModel(ScoutService scoutService)
        {
            _scoutService = scoutService;
        }
        
        [BindProperty] 
        public InputModel Input { get; set; } = new();
        
        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // Only check digits if a value was actually provided ([Required] reports the missing case)
            if (!string.IsNullOrEmpty(Input.PhoneNumber) && !Input.PhoneNumber.All(char.IsDigit))
            {
                ModelState.AddModelError("Input.PhoneNumber", "Telefonnummer må kun indeholde tal");
            }
            
            // Now this sees the error above as well as the attribute errors
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            // Building the object and pass it though the repo
            Member memberToCreate = new Member();
            memberToCreate.FirstName = Input.FirstName;
            memberToCreate.LastName = Input.LastName;
            memberToCreate.DateOfBirth = Input.DateOfBirth;
            memberToCreate.PhoneNumber = Input.PhoneNumber;
            memberToCreate.IsLeader = Input.IsLeader;
            memberToCreate.Username = Input.Username;
            memberToCreate.HashPassword = Input.Password;

            try
            {
                _scoutService.CreateMember(memberToCreate);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            
            TempData["SuccessMessage"] = $"{memberToCreate.FirstName} {memberToCreate.LastName} blev oprettet";
            return RedirectToPage("/Index");
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

            [Required(ErrorMessage = "Password mangler"), DataType(DataType.Password)]
            public string Password { get; set; } = "";

            public bool IsLeader { get; set; }
        }
    }
}