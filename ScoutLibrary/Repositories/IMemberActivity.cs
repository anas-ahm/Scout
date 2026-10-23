using ScoutLibrary.Models;

namespace ScoutLibrary.Repositories;

public interface IMemberActivity
{
    public void AssignActivityToMember(int memberID, int activitId);
    public void UnAssignActivityToMember(int memberID, int activitId);

}