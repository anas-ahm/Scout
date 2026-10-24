using MySqlConnector;
using ScoutLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data;

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
            using MySqlConnection connection = _db.Connect2();
            using MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                string query = "INSERT INTO activities (name, date_time, description, max_count, min_age) VALUES(@name, @date_time, @description, @max_count, @min_age)";
                using MySqlCommand cmd = new MySqlCommand(query, connection, transaction);
                SetActivityParameters(cmd, item);
                cmd.ExecuteNonQuery();

                // Få det nye ID som databasen netop har genereret (AUTO_INCREMENT)
                item.ActivityId = (int)cmd.LastInsertedId;


                //Indsæt badgetilknytning
                if (item.RequiredBadges != null && item.RequiredBadges.Count > 0)
                {
                    cmd.CommandText = "INSERT INTO required_activity_badges (activity_id, badge_id) VALUES (@activity_id, @badge_id)";
                    foreach (Badge badge in item.RequiredBadges)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@activity_id", item.ActivityId);
                        cmd.Parameters.AddWithValue("@badge_id", badge.BadgeId); 
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
                throw new Exception("Der opstod en fejl ved indsættelse i databasen. Prøv igen eller kontakt systemadmin" + ex.Message, ex);
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
            catch (Exception ex)
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
                    int activityId = reader.GetInt32("activity_id");                        //få id for at checke i dictionary

                    if (!activityDictionary.ContainsKey(activityId))                        //check i ordbogen for at se om aktiviteten allerede eksisterer.Gør den ikke, laves en Activity
                    {
                        activityDictionary.Add(activityId, MapActivityFromReader(reader));  //tilføjer key(id), og den læste aktivitet til dictionary
                    }

                    //hent den nuværende aktivitet for at tilføje badges og ledere:
                    Activity currentActivity = activityDictionary[activityId];

                    if (!reader.IsDBNull(reader.GetOrdinal("badge_id")))                    //nødvendigt pga left join
                    {
                        int badgeId = reader.GetInt32("badge_id");
                        if (!currentActivity.RequiredBadges.Any(b => b.BadgeId == badgeId)) //nødvendigt pga left join
                        {
                            currentActivity.RequiredBadges.Add(MapBadgeFromReader(reader)); // Tilføj badge til listen RequiredBadges i Activity
                        }
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("member_id")))                 //nødvndigt pga left join
                    {
                        int memberId = reader.GetInt32("member_id");


                        if (!currentActivity.Members.Any(m => m.MemberId == memberId))      //nødvendigt pga left join
                        {
                            currentActivity.Members.Add(MapMemberFromReader(reader));
                        }
                    }
                }
                return activityDictionary.Values.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Fejl ved indlæsning fra databasen.");
            }
        }

        public Activity GetById(int id)
        {
            Dictionary<int, Activity> activityDictionary = new Dictionary<int, Activity>();

            try
            {
                using MySqlConnection connection = _db.Connect2();
                string query = """
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
            WHERE a.activity_id = @activity_id
            """;

                using MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@activity_id", id);

                using MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int activityId = reader.GetInt32("activity_id");

                    if (!activityDictionary.ContainsKey(activityId))
                    {
                        activityDictionary.Add(activityId, MapActivityFromReader(reader));
                    }

                    Activity currentActivity = activityDictionary[activityId];

                    // Tilføj badge hvis den findes
                    if (!reader.IsDBNull(reader.GetOrdinal("badge_id")))
                    {
                        int badgeId = reader.GetInt32("badge_id");
                        if (!currentActivity.RequiredBadges.Any(b => b.BadgeId == badgeId))
                        {
                            currentActivity.RequiredBadges.Add(MapBadgeFromReader(reader));
                        }
                    }

                    // Tilføj medlem hvis det findes
                    if (!reader.IsDBNull(reader.GetOrdinal("member_id")))
                    {
                        int memberId = reader.GetInt32("member_id");
                        if (!currentActivity.Members.Any(m => m.MemberId == memberId))
                        {
                            currentActivity.Members.Add(MapMemberFromReader(reader));
                        }
                    }
                }

                // Returner aktiviteten fra ordbogen, eller null hvis den ikke blev fundet
                return activityDictionary.Values.FirstOrDefault();
            }
            catch(Exception ex)
            {
                throw new Exception("Fejl ved indlæsning af databasen");
            }
            
        }

        public Activity Update(Activity activity, int id)
        {
            using MySqlConnection connection = _db.Connect2();
            using MySqlTransaction transaction = connection.BeginTransaction();

            try
            {
                //Opdater selve aktiviteten
                string activityQuery = """
                UPDATE activities 
                SET name = @name, date_time = @datetime, description = @description, max_count = @maxcount, min_age=@minage
                WHERE activity_id=@activityid
                """;
                using MySqlCommand cmd = new MySqlCommand(activityQuery, connection, transaction);
                SetActivityParameters(cmd, activity);
                cmd.ExecuteNonQuery();


                //Sletter gamle tilknytninger i databasen
                cmd.CommandText = """
                    DELETE FROM activity_members
                    WHERE activity_id=@activity_id
                    """;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@activity_id", id);
                cmd.ExecuteNonQuery();

                cmd.CommandText = """
                    DELETE FROM required_activity_badges
                    WHERE activity_id=@activity_id
                    """;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@activity_id", id);
                cmd.ExecuteNonQuery();

                //Tilføjer nye tilknytninger til de to junction tables for members og badges HVIS listerne ikke er null og .Count>0
                InsertMembers(cmd, id, activity.Members);    
                InsertBadges(cmd, id, activity.RequiredBadges);
            
                transaction.Commit();
                activity.ActivityId = id;
                return activity;
            }
            catch (Exception ex)
            {
                //rollback hvis der opstår fejl
                transaction.Rollback();
                throw new Exception("Fejl under opdatering i databasen.", ex);
            }


        }
        #region Helper Methods (Mapping & Database actions)
        private Activity MapActivityFromReader(MySqlDataReader reader) //bruges i left join tabellerne
        {
            return new Activity
            {
                ActivityId = reader.GetInt32("activity_id"),
                Name = reader.GetString("activity_name"),
                DateTime = reader.GetDateTime("date_time"),
                Description = reader.IsDBNull(reader.GetOrdinal("activity_description")) ? null : reader.GetString("activity_description"),
                MaxCount = reader.IsDBNull(reader.GetOrdinal("max_count")) ? null : reader.GetInt32("max_count"),
                MinAge = reader.IsDBNull(reader.GetOrdinal("min_age")) ? null : reader.GetInt32("min_age"),
                RequiredBadges = new List<Badge>(),
                Members = new List<Member>()
            };
        }

        private Member MapMemberFromReader(MySqlDataReader reader)
        {
            return new Member
            {
                MemberId = reader.GetInt32("member_id"),
                FirstName = reader.GetString("first_name"),
                LastName = reader.GetString("last_name"),
                DateOfBirth = reader.GetDateTime("date_of_birth"),
                PhoneNumber = reader.GetString("phone_number"),
                IsLeader = reader.GetBoolean("is_leader"),
                Username = reader.GetString("username")
            };
        }

        private Badge MapBadgeFromReader(MySqlDataReader reader)
        {
            return new Badge
            {
                BadgeId = reader.GetInt32("badge_id"),
                Name = reader.GetString("badge_name"),
                Description = reader.IsDBNull(reader.GetOrdinal("badge_description")) ? null : reader.GetString("badge_description")
            };
        }

        private void SetActivityParameters(MySqlCommand cmd, Activity item)
        {
            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@date_time", item.DateTime);
            cmd.Parameters.AddWithValue("@description", (object?)item.Description ?? DBNull.Value); //property skal castes til object, således datatypen i property er samme type som DBNull, som ska lsendes til databasen hvis en værdi er null
            cmd.Parameters.AddWithValue("@max_count", (object?)item.MaxCount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@min_age", (object?)item.MinAge ?? DBNull.Value);
        }

        private void InsertMembers(MySqlCommand cmd, int activityId, List<Member> members)
        {
            if (members == null || members.Count == 0) return;

            cmd.CommandText = "INSERT INTO activity_members (activity_id, member_id) VALUES (@activity_id, @member_id)";
            foreach (Member m in members)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@activity_id", activityId);
                cmd.Parameters.AddWithValue("@member_id", m.MemberId);
                cmd.ExecuteNonQuery();
            }
        }
        private void InsertBadges(MySqlCommand cmd, int activityId, List<Badge> badges)
        {
            if (badges == null || badges.Count == 0) return;

            cmd.CommandText = "INSERT INTO required_activity_badges (activity_id, badge_id) VALUES (@activity_id, @badge_id)";
            foreach (Badge badge in badges)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@activity_id", activityId);
                cmd.Parameters.AddWithValue("@badge_id", badge.BadgeId);
                cmd.ExecuteNonQuery();
            }

        }
        #endregion
    }
}

//****************GetOrdinal*****************************************************************
//når du skal tjekke for NULL-værdier ved hjælp af metoden IsDBNull(),
//accepterer IsDBNUll() kun et heltal (kolonnens indeks, f.eks. kolonne 0, 1, 2 osv.) – den modtager ikke tekst.
//Det er her GetOrdinal kommer ind i billedet som en brobygger:
//Du beder GetOrdinal om at finde nummeret på f.eks. "activity_description".
//Den kigger på din SQL-forespørgsel, tæller sig frem og finder ud af, at activity_description f.eks. er kolonne nr. 3.
//Den sender det tal videre til IsDBNull(3), som så tjekker om feltet er tomt.

