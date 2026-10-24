using ScoutLibrary.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Activity = ScoutLibrary.Models.Activity;
using System.Security.Policy;
using Microsoft.Data.SqlClient;
using System.Data;

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
            using MySqlConnection connection = _db.Connect2();
            using MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                string query = "INSERT INTO activities (name, date_time, description, max_count, min_age) VALUES(@name, @date_time, @description, @max_count, @min_age)";
                using MySqlCommand cmd = new MySqlCommand(query, connection, transaction);
                
                cmd.Parameters.AddWithValue("@name", item.Name);
                cmd.Parameters.AddWithValue("@date_time", item.DateTime);
                cmd.Parameters.AddWithValue("@description", (object)item.Description??DBNull.Value);  //property skal castes til object, således datatypen i property er samme type som DBNull, som ska lsendes til databasen hvis en værdi er null
                cmd.Parameters.AddWithValue("@max_count", (object)item.MaxCount??DBNull.Value);
                cmd.Parameters.AddWithValue("@min_age", (object)item.MinAge??DBNull.Value);
                cmd.ExecuteNonQuery();

                // Få det nye ID som databasen netop har genereret (AUTO_INCREMENT)
                item.ActivityId = (int)cmd.LastInsertedId;


                //indsæt badge tilknytning. Vi bruger cmd.CommantText for at genbruge samme MySqlCommand med ny query string
                if (item.RequiredBadges != null && item.RequiredBadges.Count > 0)
                {
                    cmd.CommandText = "INSERT INTO required_activity_badges (activity_id, badge_id) VALUES (@activity_id, @badge_id)";
                    foreach (Badge badge in item.RequiredBadges)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@activity_id", item.ActivityId);
                        cmd.Parameters.AddWithValue("@badge_id", badge.BadgeId); // Kun ID'et!
                        cmd.ExecuteNonQuery();
                    }
                }

                //Indæt ledertilknytning
                if (item.Members != null && item.Members.Count > 0)
                {
                    cmd.CommandText = "INSERT INTO activity_members (activity_id, member_id) VALUES (@activity_id, @member_id)";
                    foreach (Member m in item.Members)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@activity_id", item.ActivityId);
                        cmd.Parameters.AddWithValue("@member_id", m.MemberId);
                        cmd.ExecuteNonQuery();
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

        public Activity Delete(int id) 
        {
            Activity activityToDelete = GetById(id);
            if (activityToDelete == null)
            {
                return null; // Eller kast en fejl, hvis den ikke findes
            }
            try
            {
                using MySqlConnection connection = _db.Connect2();
                string deleteQuery = "DELETE FROM activities WHERE activity_id = @activityId";
                using MySqlCommand cmd = new MySqlCommand(deleteQuery, connection);
                cmd.Parameters.AddWithValue("@activityId", id);
                cmd.ExecuteNonQuery();
            
            }
            catch(Exception ex)
            {
                throw new Exception("Noget gik galt under sletning i databasen", ex);
            }
            return activityToDelete; 
        }

        public List<Activity> GetAll()
        {
            try
            {
                //Dictionary: holder styr på unikke aktiviteter baseret på ID. Nødvendig pga LEFT JOIN
                Dictionary<int, Activity> activityDictionary = new Dictionary<int, Activity>();

                using MySqlConnection connection = _db.Connect2();

                string activitiesQuery = """
                SELECT a.activity_id, a.date_time, a.description AS activity_description, a.max_count, a.min_age, a.name AS activity_name,                                    
                        ab.badge_id,                                    
                        b.name AS badge_name, b.description AS badge_description,                                   
                        am.member_id,
                        m.first_name, m.last_name, m.date_of_birth, m.phone_number, m.is_leader, m.username
                FROM activities a 
                LEFT JOIN activity_members am ON a.activity_id = am.activity_id
                LEFT JOIN members m ON am.member_id = m.member_id
                LEFT JOIN required_activity_badges ab ON a.activity_id = ab.activity_id
                LEFT JOIN badges b ON ab.badge_id = b.badge_id                 
                """;
                using MySqlCommand activitiesCmd = new MySqlCommand(activitiesQuery, connection);
                using MySqlDataReader reader = activitiesCmd.ExecuteReader();


                while (reader.Read())
                {
                    //Hent activity_id fra databasen (key-value!), 
                    int activityId = reader.GetInt32("activity_id");

                    //check i ordbogen for at se om aktiviteten allerede eksisterer.Gør den ikke, laves en Activity
                    if (!activityDictionary.ContainsKey(activityId))
                    {
                        Activity a = new Activity
                        {
                            ActivityId = activityId,
                            Name = reader.GetString("activity_name"),
                            DateTime = reader.GetDateTime("date_time"),

                            //check for DBNull før vi læser. Se bunden for forklaring af GetOrdinal
                            Description = reader.IsDBNull(reader.GetOrdinal("activity_description")) ? null : reader.GetString("activity_description"),
                            MaxCount = reader.IsDBNull(reader.GetOrdinal("max_count")) ? null : reader.GetInt32("max_count"),
                            MinAge = reader.IsDBNull(reader.GetOrdinal("min_age")) ? null : reader.GetInt32("min_age"),

                            //opretter tomme lister, som vi fylder op senere
                            RequiredBadges = new List<Badge>(),
                            Members = new List<Member>()

                        };
                        activityDictionary.Add(activityId, a);
                    }
                    Activity currentActivity = activityDictionary[activityId]; //hent den nuværende aktivitet for at tilføje badges og ledere:

                    if (!reader.IsDBNull(reader.GetOrdinal("badge_id")))    //check først for null værdier (kanopstå pga left join
                    {
                        int badgeId = reader.GetInt32("badge_id");      //læs herefter værdiern for badge_id

                        //check om den aktivitet, vi er igang med at oprette, allerede har det badge! igen, nødvendigt pga mange left joins
                        if (!currentActivity.RequiredBadges.Any(b => b.BadgeId == badgeId))
                        {
                            Badge badge = new Badge
                            {
                                BadgeId = badgeId,
                                Name = reader.GetString("badge_name"),
                                Description = reader.IsDBNull(reader.GetOrdinal("badge_description")) ? null : reader.GetString("badge_description")
                            };
                            currentActivity.RequiredBadges.Add(badge); // Tilføj badge til listen RequiredBadges i Activity
                        }
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("member_id")))
                    {
                        int memberId = reader.GetInt32("member_id");

                        //check at der i currentActivity ikke findes en member med det id. Nødvendigt pga left join
                        if (!currentActivity.Members.Any(m => m.MemberId == memberId))
                        {
                            Member member = new Member
                            {
                                MemberId = memberId,
                                FirstName = reader.GetString("first_name"),
                                LastName = reader.GetString("last_name"),
                                DateOfBirth = reader.GetDateTime("date_of_birth"),
                                PhoneNumber = reader.GetString("phone_number"),
                                IsLeader = reader.GetBoolean("is_leader"),
                                Username = reader.GetString("username"),
                            };
                            currentActivity.Members.Add(member);
                        }
                    }
                }
                return activityDictionary.Values.ToList();
            }
            catch(Exception ex)
            {
                throw new Exception("Fejl ved indlæsning fra databasen.");
            }
        }

        public Activity GetById(int id)
        {
            Activity activity=null;

            using MySqlConnection connection = _db.Connect2();
            string query = """
                SELECT * FROM activities 
                WHERE activity_id=@activity_id
                """;
            using MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@activity_id", id);

            using MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                activity = new Activity
                {
                    ActivityId =id,
                    Name = reader.GetString("name"),
                    DateTime = reader.GetDateTime("date_time"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                    MaxCount = reader.IsDBNull(reader.GetOrdinal("max_count")) ? null : reader.GetInt32("max_count"),
                    MinAge = reader.IsDBNull(reader.GetOrdinal("min_age")) ? null : reader.GetInt32("min_age"),
                    RequiredBadges = new List<Badge>(),
                    Members = new List<Member>()
                };
            }

            if (activity == null)
            {
                return null;
            }

            //hent members
            cmd.CommandText= """
                    SELECT m.* 
                    FROM activity_members am
                    JOIN members m ON am.member_id = m.member_id
                    WHERE am.activity_id = @activity_id
                """;
            
            cmd.Parameters.AddWithValue("@activity_id", id);            
            while (reader.Read())
            {
                Member member = new Member
                {
                    MemberId = reader.GetInt32("member_id"),
                    FirstName = reader.GetString("first_name"),
                    LastName = reader.GetString("last_name"),
                    DateOfBirth = reader.GetDateTime("date_of_birth"),
                    PhoneNumber = reader.GetString("phone_number"),
                    IsLeader = reader.GetBoolean("is_leader"),
                    Username = reader.GetString("username"),
                };
                activity.Members.Add(member);           
            }
            
            //hent badges
            cmd.CommandText = """
                SELECT b.*
                FROM required_activity_badges ab
                JOIN badges b ON ab.badge_id = b.badge_id
                WHERE activity_id = @activity_id
                """;
            cmd.Parameters.AddWithValue("@activity_id", id);
            while (reader.Read())
            {
                          
                Badge b = new Badge
                {
                    BadgeId = reader.GetInt32("badge_id"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                    Name = reader.GetString("name")
                };
                activity.RequiredBadges.Add(b);
            }
            return activity;
        }

        public Activity Update(Activity newItem, int id)
        {
            using(MySqlConnection connection = _db.Connect2())
            {
                using(MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        //Opdater selve aktiviteten
                        string activityQuery = """
                        UPDATE activities 
                        SET name = @name, date_time = @datetime, description = @description, max_count = @maxcount, min_age=@minage
                        WHERE activity_id=@activityid
                        """;
                        using (MySqlCommand activityCmd = new MySqlCommand(activityQuery, connection, transaction))
                        {
                            activityCmd.Parameters.AddWithValue("@activityid", id);
                            activityCmd.Parameters.AddWithValue("@name", newItem.Name);
                            activityCmd.Parameters.AddWithValue("@datetime", newItem.DateTime);
                            activityCmd.Parameters.AddWithValue("@description", (object?)newItem.Description ?? DBNull.Value);
                            activityCmd.Parameters.AddWithValue("@maxcount", (object?)newItem.MaxCount ?? DBNull.Value);
                            activityCmd.Parameters.AddWithValue("@minage", (object?)newItem.MinAge ?? DBNull.Value);
                            activityCmd.ExecuteNonQuery();
                        }

                        //Sletter gamle tilknytninger i databasen
                        string deleteActMembQuery = """
                            DELETE FROM activity_members
                            WHERE activity_id=@activity_id
                            """;
                        string deleteActBadgeQuery = """
                            DELETE FROM required_activity_badges
                            WHERE activity_id=@activity_id
                            """;
                        using (MySqlCommand actMembCmd = new MySqlCommand(deleteActMembQuery, connection, transaction))
                        {
                            actMembCmd.Parameters.AddWithValue("@activity_id", id);
                            actMembCmd.ExecuteNonQuery();
                        }
                        using (MySqlCommand actMembCmd = new MySqlCommand(deleteActBadgeQuery, connection, transaction))
                        {
                            actMembCmd.Parameters.AddWithValue("@activity_id", id);
                            actMembCmd.ExecuteNonQuery();
                        }

                        //Tilføjer nye tilknytninger til de to junction tables for members og badges
                        string addMembersQuery = """
                            INSERT INTO activity_members (activity_id, member_id)
                            VALUES  (@activity_id, @member_id)
                            """;
                        if (newItem.Members != null && newItem.Members.Count > 0)
                        {
                            using (MySqlCommand membCmd = new MySqlCommand(addMembersQuery, connection, transaction))
                            {
                                foreach (Member m in newItem.Members)
                                {
                                    membCmd.Parameters.Clear();
                                    membCmd.Parameters.AddWithValue("@activity_id", id);
                                    membCmd.Parameters.AddWithValue("@member_id", m.MemberId);
                                    membCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        string addBadgesQuery = """
                            INSERT INTO required_activity_badges (activity_id, badge_id)
                            VALUES (@activity_id, @badge_id)
                            """;
                        if(newItem.RequiredBadges!=null && newItem.RequiredBadges.Count > 0)
                        {
                            using (MySqlCommand badgeCmd = new MySqlCommand(addBadgesQuery, connection, transaction))
                            {
                                foreach (Badge b in newItem.RequiredBadges)
                                {
                                    badgeCmd.Parameters.Clear();
                                    badgeCmd.Parameters.AddWithValue("@activity_id", id);
                                    badgeCmd.Parameters.AddWithValue("@badge_id", b.BadgeId);
                                    badgeCmd.ExecuteNonQuery();
                                }

                            }
                        }
                        //commit hvis alt går godt
                        transaction.Commit();

                        newItem.ActivityId = id;
                        return newItem;
                    }
                    catch(Exception ex)
                    {
                        //rollback hvis der opstår fejl
                        transaction.Rollback();
                        throw new Exception("Fejl under opdatering i databasen.", ex);
                    }
                  
                }
            }
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