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
                                    cmdBadge.Parameters.Clear(); //denne linje er nødvendig fordi vi kører en loop og skal indsætte nye værdier for hver iteration
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
            //Dictionary: holder styr på unikke aktiviteter baseret på ID. Nødvendig pga LEFT JOIN
            Dictionary<int, Activity> activityDictionary = new Dictionary<int, Activity>();
            
            using (MySqlConnection connection = _db.Connect2())
            {
                string activitiesQuery = """
                    SELECT a.activity_id, a.date_time, a.description AS activity_description, a.max_count, a.min_age, a.name AS activity_name, ab.badge_id, b.name AS badge_name, b.description AS badge_description
                    FROM activities a 
                    LEFT JOIN required_activity_badges ab ON a.activity_id=ab.activity_id
                    LEFT JOIN badges b ON ab.badge_id=b.badge_id
                    """;
                using MySqlCommand activitiesCmd = new MySqlCommand(activitiesQuery, connection);
                using MySqlDataReader reader = activitiesCmd.ExecuteReader();
                              

                while (reader.Read())
                {
                    //Hent activity_id fra databasen (key-value!), check i ordbogen for at se om aktiviteten allerede eksisterer. Gør den ikke, laves en Activity
                    int activityId = reader.GetInt32("activity_id");
                    if (!activityDictionary.ContainsKey(activityId))
                    {
                        Activity a = new Activity
                        {
                            ActivityId = activityId,
                            Name = reader.GetString("activity_name"),
                            DateTime = reader.GetDateTime("date_time"),

                            //check for DBNull før vi læser. Se bunden for forklaring af GetOrdinal
                            Description = reader.IsDBNull(reader.GetOrdinal("activity_description")) ? null : reader.GetString("description"),
                            MaxCount = reader.IsDBNull(reader.GetOrdinal("max_count")) ? null : reader.GetInt32("max_count"),
                            MinAge = reader.IsDBNull(reader.GetOrdinal("min_age")) ? null : reader.GetInt32("min_age"),

                            //opretter tomme lister, som vi fylder op senere
                            RequiredBadges = new List<Badge>(),
                            Members = new List<Member>()

                        };

                        activityDictionary.Add(activityId,a);
                    }


                    //check om der er et badge tilknyttet i denne række (kan være null pga left join)
                    if (!reader.IsDBNull(reader.GetOrdinal("badge_id")))
                    {
                        Badge badge = new Badge
                        {
                            BadgeId = reader.GetInt32("badge_id"),
                            Name = reader.GetString("badge_name"), 
                            Description = reader.IsDBNull(reader.GetOrdinal("badge_description")) ? null : reader.GetString("description")
                        };

                        // Tilføj badge til listen RequiredBadges i Activity.
                        activityDictionary[activityId].RequiredBadges.Add(badge);
                    }

                }
                List<Activity> activities = activityDictionary.Values.ToList();


            }
            return activityDictionary.Values.ToList();
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

//****************GetOrdinal*****************************************************************
//når du skal tjekke for NULL-værdier ved hjælp af metoden IsDBNull(),
//accepterer IsDBNUll() kun et heltal (kolonnens indeks, f.eks. kolonne 0, 1, 2 osv.) – den modtager ikke tekst.
//Det er her GetOrdinal kommer ind i billedet som en brobygger:
//Du beder GetOrdinal om at finde nummeret på f.eks. "activity_description".
//Den kigger på din SQL-forespørgsel, tæller sig frem og finder ud af, at activity_description f.eks. er kolonne nr. 3.
//Den sender det tal videre til IsDBNull(3), som så tjekker om feltet er tomt.