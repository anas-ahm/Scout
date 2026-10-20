using ScoutLibrary.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Activity = ScoutLibrary.Models.Activity;

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
            using (MySqlConnection connection = _db.Connect())
            {
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        long newActivityId = 0;

                        string query = "INSERT INTO activities (name, date_time, description, max_count, min_age) VALUES(@name, @date_time, @description, @max_count, @min_age)";
                        using (MySqlCommand cmd = new MySqlCommand(query, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@name", item.Name);
                            cmd.Parameters.AddWithValue("@date_time", item.DateTime);
                            cmd.Parameters.AddWithValue("@description", item.Description);
                            cmd.Parameters.AddWithValue("@max_count", item.MaxCount);
                            cmd.Parameters.AddWithValue("@min_age", item.MinAge);
                            cmd.ExecuteNonQuery();


                            // Få det nye ID som databasen netop har genereret (AUTO_INCREMENT)
                            newActivityId = cmd.LastInsertedId;
                            item.ActivityId = (int)newActivityId;
                        }

                        string reqBadgeQuery = "INSERT INTO required_activity_badges (activity_id, badge_id) VALUES (@activity_id, @badge_id)";
                        using (MySqlCommand cmdBadge = new MySqlCommand(reqBadgeQuery, connection, transaction))
                        {
                            for (int i = 0; i < item.RequiredBadges.Count; i++)
                            {
                                cmdBadge.Parameters.AddWithValue("@activity_id", item.ActivityId);
                                cmdBadge.Parameters.AddWithValue("@badge_id", item.RequiredBadges[i]);
                                cmdBadge.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                        return item;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Der opstod en fejl ved indsættelse i databasen. Prøv igen eller kontakt systemadmin" + ex.Message,ex);
                    }
                }
                    
            }

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
