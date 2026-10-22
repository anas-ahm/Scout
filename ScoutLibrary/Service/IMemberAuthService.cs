using ScoutLibrary.Models;

namespace ScoutLibrary.Service;

public interface IMemberAuthService
{
	public Member GetMemberByUsername(string username);

	public Member Login(string username, string password);

	public Member Register(Member member);
}
