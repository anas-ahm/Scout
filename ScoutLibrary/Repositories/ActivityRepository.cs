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
            using (MySqlConnection connection = _db.Connect2())
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
                            cmd.Parameters.AddWithValue("@description", (object)item.Description??DBNull.Value);  //property skal castes til object, således datatypen i property er samme type som DBNull, som ska lsendes til databasen hvis en værdi er null
                            cmd.Parameters.AddWithValue("@max_count", (object)item.MaxCount??DBNull.Value);
                            cmd.Parameters.AddWithValue("@min_age", (object)item.MinAge??DBNull.Value);
                            cmd.ExecuteNonQuery();



                            // Få det nye ID som databasen netop har genereret (AUTO_INCREMENT)
                            newActivityId = cmd.LastInsertedId;
                            item.ActivityId = (int)newActivityId;
                        }

                        string reqBadgeQuery = "INSERT INTO required_activity_badges (activity_id, badge_id) VALUES (@activity_id, @badge_id)";
                        using (MySqlCommand cmdBadge = new MySqlCommand(reqBadgeQuery, connection, transaction))
                        {
                            if (item.RequiredBadges != null&&item.RequiredBadges.Count>0)
                            {
                                for (int i = 0; i < item.RequiredBadges.Count; i++)
                                {
                                    cmdBadge.Parameters.Clear();
                                    cmdBadge.Parameters.AddWithValue("@activity_id", item.ActivityId);
                                    cmdBadge.Parameters.AddWithValue("@badge_id", (object)item.RequiredBadges[i] ?? DBNull.Value);
                                    cmdBadge.ExecuteNonQuery();
                                }
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
