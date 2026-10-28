using ScoutLibrary.Models;
using ScoutLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScoutLibrary.Service
{
    public class ActivityService
    {
        private readonly ICRUD<Activity> _activityRepository;

        public ActivityService(ICRUD<Activity> activityRepository)
        {
            _activityRepository = activityRepository;
        }

        public Activity Create(Activity item)
        {
            return _activityRepository.Create(item);            
        }

        public List<Activity> GetAll()
        {
            return _activityRepository.GetAll();
        }

        public Activity GetById(int id)
        {
            return _activityRepository.GetById(id);
        }

        public Activity Delete(int id)
        {
            return _activityRepository.Delete(id);
        }

        public Activity Update(Activity item, int id)
        {
            return _activityRepository.Update(item, id);
        }
        public List<Activity> filter(string SearchQuery, List<int?> AssignedActivityIds, bool ShowOnlyAssigned, Member LoggedInMember)
        {
            List<Activity> allActivities = GetAll();
            if (ShowOnlyAssigned && LoggedInMember != null)
            {
                allActivities = allActivities.Where(a => AssignedActivityIds.Contains(a.ActivityId)).ToList();
            }


            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                return allActivities;
            }
            else
            {
                // Check om søgeordet er et tal (søgning efter alder)
                bool isNumeric = int.TryParse(SearchQuery, out int searchedAge);

                return allActivities.Where(a =>

                    (a.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) //filtrerer efter navn
                    ||
                    (isNumeric && a.MinAge.HasValue && a.MinAge.Value == searchedAge) //filterer efter alder (kun hvis det er et tal)
                    ||
                    (a.RequiredBadges != null && a.RequiredBadges.Any(b => b.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))) // Filtrerer efter tilknyttede badges
                ).ToList();
            }
        }
    }
}
