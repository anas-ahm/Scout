using ScoutLibrary.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Activity = ScoutLibrary.Models.Activity;
using System.Security.Policy;

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


                        //indsætter tilknyttede badges til junction tabellen required_activitybadges
                        string requiredActivityBadgesQuery = "INSERT INTO required_activity_badges (activity_id, badge_id) VALUES (@activity_id, @badge_id)";
                        using (MySqlCommand cmdBadge = new MySqlCommand(requiredActivityBadgesQuery, connection, transaction))
                        {
                            if (item.RequiredBadges != null && item.RequiredBadges.Count > 0)
                            {
                                foreach (Badge badge in item.RequiredBadges)
                                {
                                    cmdBadge.Parameters.Clear();
                                    cmdBadge.Parameters.AddWithValue("@activity_id", item.ActivityId);
                                    cmdBadge.Parameters.AddWithValue("@badge_id", badge.BadgeId); // Kun ID'et!
                                    cmdBadge.ExecuteNonQuery();
                                }
                            }
                        }
                     
                        //indsætter tilknyttede ledere til junction tabellen activity_members
                        string activityMembersQuery = "INSERT INTO activity_members (activity_id, member_id) VALUES (@activity_id, @member_id)";
                        using (MySqlCommand memberCmd = new MySqlCommand(activityMembersQuery, connection, transaction))
                        {
                            if (item.Members != null && item.Members.Count > 0)
                            {
                                foreach (Member m in item.Members)
                                {
                                    memberCmd.Parameters.Clear();
                                    memberCmd.Parameters.AddWithValue("@activity_id", item.ActivityId);
                                    memberCmd.Parameters.AddWithValue("@member_id", m.MemberId); 
                                    memberCmd.ExecuteNonQuery();
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

        public Activity Delete(int id) //skifter til Activity når GetById er lavet
        {
            //Activity activityToDelete = GetById(id);
            //if (activityToDelete == null)
            //{
            //    return null; // Eller kast en fejl, hvis den ikke findes
            //}
            try
            {
                using (MySqlConnection connection = _db.Connect2())
                {
                    string deleteQuery = "DELETE FROM activities WHERE activity_id = @activityId";
                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@activityId", id);
                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Noget gik galt under sletning i databasen", ex);
            }
            return null; //afleverer senere et rigtigt objekt
        }

        public List<Activity> GetAll()
        {
            //Dictionary: holder styr på unikke aktiviteter baseret på ID. Nødvendig pga LEFT JOIN
            Dictionary<int, Activity> activityDictionary = new Dictionary<int, Activity>();
            
            using (MySqlConnection connection = _db.Connect2())
            {
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
                            Description = reader.IsDBNull(reader.GetOrdinal("activity_description")) ? null : reader.GetString("activity_description"),
                            MaxCount = reader.IsDBNull(reader.GetOrdinal("max_count")) ? null : reader.GetInt32("max_count"),
                            MinAge = reader.IsDBNull(reader.GetOrdinal("min_age")) ? null : reader.GetInt32("min_age"),

                            //opretter tomme lister, som vi fylder op senere
                            RequiredBadges = new List<Badge>(),
                            Members = new List<Member>()

                        };


                        activityDictionary.Add(activityId, a);
                    }

                    //hent den nuværende aktivitet for at tilføje badges og ledere:
                    Activity currentActivity = activityDictionary[activityId];


                    //check først for null værdier (kanopstå pga left join
                    if (!reader.IsDBNull(reader.GetOrdinal("badge_id")))
                    {
                        //læs herefter værdiern for badge_id
                        int badgeId = reader.GetInt32("badge_id");

                        //check om den aktivitet, vi er igang med at oprette, allerede har det badge! igen, nødvendigt pga mange left joins
                        if (!currentActivity.RequiredBadges.Any(b => b.BadgeId == badgeId))
                        {
                            Badge badge = new Badge
                            {
                                BadgeId = badgeId,
                                Name = reader.GetString("badge_name"),
                                Description = reader.IsDBNull(reader.GetOrdinal("badge_description")) ? null : reader.GetString("badge_description")
                            };

                            // Tilføj badge til listen RequiredBadges i Activity
                            currentActivity.RequiredBadges.Add(badge);
                        }                       
                                                
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("member_id")))
                    {
                        //læs værdien for memberId
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