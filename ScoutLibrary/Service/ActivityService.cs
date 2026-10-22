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
    }
}
