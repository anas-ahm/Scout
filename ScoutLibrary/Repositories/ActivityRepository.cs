using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ScoutLibrary.Repositories
{
    
    public class ActivityRepository : ICRUD<Activity>
    {
        private readonly DBWorker _db;
        public ActivityRepository(DBWorker db)
        {
            _db = db;
        }

        public Activity Create(Activity item)
        {
            
            throw new NotImplementedException();
        }

        public Activity Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Activity> GetAll()
        {
            throw new NotImplementedException();
        }

        public Activity GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Activity Update(Activity newItem, int id)
        {
            throw new NotImplementedException();
        }
    }
}
