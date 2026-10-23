using ScoutLibrary.Models;
using ScoutLibrary.Repositories;

namespace ScoutLibrary.Service;

public class ScoutService
{
    private readonly ICRUD<Member> _memberRepo;
    private readonly ICRUD<Activity> _activityRepo;
    private readonly IMemberActivity _memberActivity;
    
    public ScoutService(ICRUD<Member> memberRepo, ICRUD<Activity> activityRepo, IMemberActivity memberActivity)
    {
        _memberRepo = memberRepo;
        _activityRepo = activityRepo;
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

    public void AssignActivityToMember(int memberId, int activityId)
    {
        Activity? activityToCheck = _activityRepo.GetById(activityId);
        Member? member = _memberRepo.GetById(memberId);

        try
        {
            if (activityToCheck is null)
            {
                throw new KeyNotFoundException("Aktiviteten blev ikke fundet");
            }

            if (activityToCheck.DateTime < DateTime.Now)
            {
                throw new InvalidOperationException("Du kan ikke tilmelde dig en aktivitet i fortiden");
            }

            if (member.Activities.Any(a => a.ActivityId == activityId))
            {
                throw new InvalidOperationException("Du er allerede tilmeldt denne aktivitet");
            }
            
            if (activityToCheck.Members.Count >= activityToCheck.MaxCount)
            {
                throw new InvalidOperationException("Aktiviteten er allerede fuld");
            }
        }
        
        // If there's another exception than the ones defiend above, we throw a new generic one
        // We do this so the ones above can be see in the UI layer.
        catch (Exception ex) when (ex is not (KeyNotFoundException or InvalidOperationException)) 
        {
            throw new Exception("Couldnt assign actitvity to member:", ex);
        }
        
        
        _memberActivity.AssignActivityToMember(memberId, activityId);
    }
    
    public void UnAssignActivityToMember(int memberID, int activitId)
    {
        _memberActivity.UnAssignActivityToMember(memberID, activitId);
    }

}
