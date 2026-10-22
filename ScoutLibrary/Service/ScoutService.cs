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

    public List<Member> ReadAllMembers()
    {
        return _memberRepo.GetAll();
    }

    public Member DeleteMember(int id)
    {
        return _memberRepo.Delete(id);
    }

    public Member GetById(int id)
    {
        return _memberRepo.GetById(id);
    }

}
