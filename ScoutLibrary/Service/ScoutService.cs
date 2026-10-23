using ScoutLibrary.Models;
using ScoutLibrary.Repositories;

namespace ScoutLibrary.Service;

public class ScoutService
{
    private readonly ICRUD<Member> _memberRepo;
    private readonly IMemberActivity _memberActivity;
    
    public ScoutService(ICRUD<Member> memberRepo, IMemberActivity memberActivity)
    {
        _memberRepo = memberRepo;
        _memberActivity = memberActivity;
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

    public Member Update(Member member, int id)
    {
        return _memberRepo.Update(member, id);
    }

    public void AssignActivityToMember(int memberID, int activitId)
    {
        _memberActivity.AssignActivityToMember(memberID, activitId);
    }
    
    public void UnAssignActivityToMember(int memberID, int activitId)
    {
        _memberActivity.UnAssignActivityToMember(memberID, activitId);
    }

}
