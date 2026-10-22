using ScoutLibrary.Models;
using ScoutLibrary.Repositories;

namespace ScoutLibrary.Service;

public class ScoutService
{
    private readonly ICRUD<Member> _memberRepo;
    
    public ScoutService(ICRUD<Member> memberRepo)
    {
        _memberRepo = memberRepo;
    }

    public Member CreateMember(Member member)
    {
        return _memberRepo.Create(member);
    }

}
