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
}
