using ScoutLibrary.Models;

namespace ScoutLibrary.Repositories;

public interface IMemberAuth
{
	public Member? GetByUsername(string username);
}
