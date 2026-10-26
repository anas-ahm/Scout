using System.Security.Claims;
using ScoutLibrary.Models;

namespace ScoutLibrary.Service;

public interface IMemberAuthService
{
	public Member? GetMemberByUsername(string username);

	public Member Login(string username, string password);

	public Member Register(Member member);

	public ClaimsPrincipal LoginUser(Member member);

	public ClaimsPrincipal RegisterUser(Member member);
}
