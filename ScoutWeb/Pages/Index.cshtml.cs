using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScoutLibrary.Models;
using ScoutLibrary.Repositories;
using System.Security.Claims;

namespace ScoutWeb.Pages;

public class IndexModel : PageModel
{
    private readonly ICRUD<Member> _memberRepository;
    public Member Member { get; set; }

    public IndexModel(ICRUD<Member> memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public void OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                int id = int.Parse(userId);

                Member = _memberRepository.GetById(id);
            }
    }

    public void OnPostCreateScout()
    {
        
    }
}