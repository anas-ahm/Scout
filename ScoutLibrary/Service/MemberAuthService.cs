using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using ScoutLibrary.Models;
using ScoutLibrary.Repositories;

namespace ScoutLibrary.Service;

public class MemberAuthService : IMemberAuthService
{
	private readonly IMemberAuth _memberAuth;
	private readonly ICRUD<Member> _memberRepository;

	private readonly PasswordHasher<Member> _passwordHasher = new PasswordHasher<Member>();

	public MemberAuthService(IMemberAuth memberAuth, ICRUD<Member> memberRepository)
	{
		_memberAuth = memberAuth;
		_memberRepository = memberRepository;
	}

	public Member? GetMemberByUsername(string username)
	{
		return _memberAuth.GetByUsername(username);
	}

	public Member Login(string username, string password)
	{
		Member? member = _memberAuth.GetByUsername(username);

		// IT Security: prevent enumeration attacks by throwing a generic exception for both invalid username and password
		if (member == null)
		{
			throw new UnauthorizedAccessException("Brugernavn eller adgangskode er forkert.");
		}

		// Verify the password using the PasswordHasher
		PasswordVerificationResult result =
			_passwordHasher.VerifyHashedPassword(member, member.HashPassword, password);

		// IT Security: prevent enumeration attacks by throwing a generic exception for both invalid username and password
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Brugernavn eller adgangskode er forkert.");
        }

        return member;
	}

	public Member Register(Member member)
	{
		// Hash the password before creating the member
		member.HashPassword = _passwordHasher.HashPassword(member, member.HashPassword);

		return _memberRepository.Create(member);
	}

	public ClaimsPrincipal LoginUser(Member member)
	{
		// Create claims for the logged-in user, meaning the information we want to store in the cookie
		List<Claim> claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, member.MemberId.ToString()),

			new Claim(ClaimTypes.Name, member.Username),

			new Claim(ClaimTypes.Role, member.IsLeader ? "Leader" : "Member")
		};

		// Create a ClaimsIdentity with the claims and the authentication scheme, which represents the user's identity
		ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

		// Create a ClaimsPrincipal with the identity, which represents the logged-in user
		ClaimsPrincipal principal = new ClaimsPrincipal(identity);
		return principal;
	}

	public ClaimsPrincipal RegisterUser(Member member)
	{
		// Create claims for the registered user, meaning the information we want to store in the cookie
		List<Claim> claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, member.MemberId.ToString()),

			new Claim(ClaimTypes.Name, member.Username),

			new Claim(ClaimTypes.Role, member.IsLeader ? "Leader" : "Member")
		};

		// Create a ClaimsIdentity with the claims and the authentication scheme, which represents the user's identity
		ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

		// Create a ClaimsPrincipal with the identity, which represents the registered user
		ClaimsPrincipal principal = new ClaimsPrincipal(identity);
		return principal;
	}
}
