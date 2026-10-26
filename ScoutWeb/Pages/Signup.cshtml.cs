using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoutLibrary.Models;
using ScoutLibrary.Service;

namespace ScoutWeb.Pages
{
    public class SignupModel : PageModel
    {
        private readonly IMemberAuthService _memberAuthService;

        public SignupModel(IMemberAuthService memberAuthService)
        {
            _memberAuthService = memberAuthService;
        }
        
        [BindProperty] 
        public InputModel Input { get; set; } = new();
        
        public IActionResult OnGet()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }
            return Page();
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
            memberToCreate.IsLeader = false; // Signup page is for members only, not leaders
            memberToCreate.Username = Input.Username;
            memberToCreate.HashPassword = Input.Password;

            try
            {
                Member registeredMember = _memberAuthService.Register(memberToCreate);

                // Check if the password and confirm password match
                if (Input.Password != Input.ConfirmPassword)
                {
                    ModelState.AddModelError("Input.ConfirmPassword", "Passwords matcher ikke");
                    return Page();
                }

                AuthRegister(registeredMember);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            
            TempData["SuccessMessage"] = $"Velkommen {memberToCreate.FirstName} {memberToCreate.LastName}, du er nu oprettet som spejder medlem";
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

            [Compare(nameof(Password), ErrorMessage = "Passwords matcher ikke")]
            [Required(ErrorMessage = "Bekræft password mangler"), DataType(DataType.Password)]
            public string ConfirmPassword { get; set; } = "";
        }

        public void AuthRegister(Member registeredMember)
        {
            ClaimsPrincipal principal = _memberAuthService.RegisterUser(registeredMember);

            // ASP.NET Core provides an asynchronous method.
            // GetAwaiter().GetResult() lets us call it synchronously.
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).GetAwaiter().GetResult();
        }
    }
}
