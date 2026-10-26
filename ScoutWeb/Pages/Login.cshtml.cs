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
    public class LoginModel : PageModel
    {
    private readonly IMemberAuthService _memberAuthService;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public LoginModel(IMemberAuthService memberAuthService)
        {
            _memberAuthService = memberAuthService;
        }

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
            try
            {
                Member member = _memberAuthService.Login(Input.Username, Input.Password);

                if (member == null)
                {
                    return Page();
                }
                
                // If login is successful, create the authentication cookie
                LoginAuth(member);

                TempData["SuccessMessage"] = $"Velkommen, {member.GetFullName()}! Du er nu logget ind.";
                return RedirectToPage("/Index");
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }

        }

        public class InputModel
        {
            [Required(ErrorMessage = "Brugernavn mangler")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Adgangskode mangler")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public void LoginAuth(Member member)
        {
            ClaimsPrincipal principal = _memberAuthService.LoginUser(member);

            // ASP.NET Core provides an asynchronous method.
            // GetAwaiter().GetResult() lets us call it synchronously.
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).GetAwaiter().GetResult();
        }
    }
}
