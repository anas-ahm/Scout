using ScoutLibrary.Models;
using MySqlConnector;
using Org.BouncyCastle.Cms;

namespace ScoutLibrary.Repositories
{
    public class MemberRepository : ICRUD<Member>, IMemberAuth, IMemberActivity
    {
        private readonly DBWorker _db;
        
        public MemberRepository(DBWorker db)
        {
            _db = db;
        }
        
        public Member Create(Member member)
        {
            try
            {
                string sql = """
                             INSERT INTO members (first_name, last_name, date_of_birth, phone_number, is_leader, username, password)
                             VALUES (@first_name, @last_name, @date_of_birth, @phone_number, @is_leader, @username, @password)
                             """;

                using MySqlConnection connString = _db.Connect();

                using MySqlCommand cmd = new MySqlCommand(sql, connString);

                cmd.Parameters.AddWithValue("@first_name", member.FirstName);
                cmd.Parameters.AddWithValue("@last_name", member.LastName);
                cmd.Parameters.AddWithValue("@date_of_birth", member.DateOfBirth);
                cmd.Parameters.AddWithValue("@phone_number", member.PhoneNumber);
                cmd.Parameters.AddWithValue("@is_leader", member.IsLeader);
                cmd.Parameters.AddWithValue("@username", member.Username);
                cmd.Parameters.AddWithValue("@password", member.HashPassword);
                cmd.ExecuteNonQuery();
                
                // Put the ID created by the database into the object
                long newMemberId = cmd.LastInsertedId;
                member.MemberId = (int)newMemberId;

                return member;
            }
            
            // this catch, is if username is already taken.
            catch(MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
            {
                throw new Exception("Brugernavnet er allerede i brug.", ex);
            }
            
            // remaining exceptions
            catch(Exception ex)
            {
                throw new Exception("Der opstod en fejl, da du prøvede at oprette en spejder!", ex);
            }
        }
        
        public Member Delete(int id)
        {
            int rowsDeleted;
            
            try
            {
                string sql = "DELETE FROM members WHERE member_id = @id";

                using MySqlConnection connString = _db.Connect();

                using MySqlCommand cmd = new MySqlCommand(sql, connString);

                cmd.Parameters.AddWithValue("id", id);
                rowsDeleted = cmd.ExecuteNonQuery();

            }
            
            catch (Exception ex)
            {
                throw new Exception("Fejl opstået", ex);
            }
            
            if (rowsDeleted == 0)
            {
                throw new KeyNotFoundException("Medlemmet blev ikke fundet.");
            }
            
            return new Member { MemberId = id };
        }

        // BADGE
        // BADGE 
        // BADGE
        public Member Update(Member member, int id)
        {
            try
            {
                string sql = """
                             UPDATE members 
                             SET first_name = @first_name, last_name = @last_name, date_of_birth = @date_of_birth, phone_number = @phone_number, is_leader = @is_leader, username = @username 
                             WHERE member_id = @member_id
                             """;
                using MySqlConnection connString = _db.Connect();

                using MySqlCommand cmd = new MySqlCommand(sql, connString);
                cmd.Parameters.AddWithValue("@first_name", member.FirstName);
                cmd.Parameters.AddWithValue("@last_name", member.LastName);
                cmd.Parameters.AddWithValue("@date_of_birth", member.DateOfBirth);
                cmd.Parameters.AddWithValue("@phone_number", member.PhoneNumber);
                cmd.Parameters.AddWithValue("@is_leader", member.IsLeader);
                cmd.Parameters.AddWithValue("@username", member.Username);
                cmd.Parameters.AddWithValue("@member_id", id);

                cmd.ExecuteNonQuery();
                
                member.MemberId = id;
                AssignBadgeToMember(member);
            }
            
            // this catch, is if username is already taken.
            catch(MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
            {
                throw new Exception("Brugernavnet er allerede i brug.", ex);
            }
            
            // remaining exceptions
            catch(Exception ex)
            {
                Console.WriteLine(ex);   // temporary: prints the full exception chain to the console
                throw new Exception("Der opstod en fejl, da du prøvede at opdater en spejder!", ex);
            }
            
            return member;
        }

        // BADGE
        // BADGE 
        // BADGE
        public Member? GetById(int id)
        {
            try
            {
                string sql = """
                             SELECT members.member_id, members.first_name, members.last_name, members.date_of_birth,
                                    members.phone_number, members.is_leader, members.username,
                                    activity_members.activity_id
                             FROM members
                             LEFT JOIN activity_members ON members.member_id = activity_members.member_id
                             WHERE members.member_id = @member_id
                             """;
                
                using MySqlConnection connString = _db.Connect();

                using MySqlCommand cmd = new MySqlCommand(sql, connString);
                cmd.Parameters.AddWithValue("member_id", id);

                using MySqlDataReader reader = cmd.ExecuteReader();

                Member? memberToRead = null;
                
                while (reader.Read())
                {

                    // only build the member once before loopting through activities
                    if (memberToRead is null)
                    {
                        memberToRead = new Member();
                        memberToRead.MemberId = reader.GetInt32("member_id");
                        memberToRead.FirstName = reader.GetString("first_name");
                        memberToRead.LastName = reader.GetString("last_name");
                        memberToRead.DateOfBirth = reader.GetDateTime("date_of_birth");
                        memberToRead.PhoneNumber = reader.GetString("phone_number");
                        memberToRead.IsLeader = reader.GetBoolean("is_leader");
                        memberToRead.Username = reader.GetString("username");
                    }
                    
                    // activity_id is NULL when the member has no activities (LEFT JOIN)
                    if (!reader.IsDBNull(reader.GetOrdinal("activity_id")))
                    {
                        memberToRead.Activities.Add(new Activity { ActivityId = reader.GetInt32("activity_id") });
                    }
                }
                
                // Close the first reader, then load the badges with a new query
                reader.Close();
                
                if (memberToRead is not null)
                {
                    string badgeSql = """
                                      SELECT badges.badge_id, badges.name, badges.description
                                      FROM member_badges
                                      JOIN badges ON badges.badge_id = member_badges.badge_id
                                      WHERE member_badges.member_id = @member_id
                                      """;

                    using MySqlCommand badgeCmd = new MySqlCommand(badgeSql, connString);
                    badgeCmd.Parameters.AddWithValue("@member_id", id);

                    using MySqlDataReader badgeReader = badgeCmd.ExecuteReader();

                    while (badgeReader.Read())
                    {
                        Badge badge = new Badge();
                        badge.BadgeId = badgeReader.GetInt32("badge_id");
                        badge.Name = badgeReader.GetString("name");
                        badge.Description = badgeReader.IsDBNull(badgeReader.GetOrdinal("description"))
                            ? null
                            : badgeReader.GetString("description");

                        memberToRead.Badges.Add(badge);
                    }
                }

                return memberToRead;
            }
            
            // Throws a KeyNotFoundException if member is null 
            catch (Exception ex)
            {
                throw new Exception("Noget gik galt ved at hente spejder: ", ex);
            }
        }

        // BADGE
        // BADGE 
        // BADGE
        public List<Member> GetAll()
        {
            try
            {
                string sql = """
                             SELECT member_id, first_name, last_name, date_of_birth, phone_number, is_leader, username
                             FROM members
                             ORDER BY first_name
                             """;
                
                using MySqlConnection connString = _db.Connect();

                using MySqlCommand cmd = new MySqlCommand(sql, connString);

                using MySqlDataReader reader = cmd.ExecuteReader();

                List<Member> memberList = new List<Member>();
                while (reader.Read())
                {
                    Member memberToAdd = new Member();
                    memberToAdd.MemberId = reader.GetInt32("member_id");
                    memberToAdd.FirstName = reader.GetString("first_name");
                    memberToAdd.LastName = reader.GetString("last_name");
                    memberToAdd.DateOfBirth = reader.GetDateTime("date_of_birth");
                    memberToAdd.PhoneNumber = reader.GetString("phone_number");
                    memberToAdd.IsLeader = reader.GetBoolean("is_leader");
                    memberToAdd.Username = reader.GetString("username");
                
                    memberList.Add(memberToAdd);
                }
                
                // Close the first reader, then load all member badges in ONE query
                reader.Close();
                
                string badgeSql = """
                                  SELECT member_badges.member_id, badges.badge_id, badges.name, badges.description
                                  FROM member_badges
                                  JOIN badges ON badges.badge_id = member_badges.badge_id
                                  """;
                
                using MySqlCommand badgeCmd = new MySqlCommand(badgeSql, connString);
                using MySqlDataReader badgeReader = badgeCmd.ExecuteReader();

                while (badgeReader.Read())
                {
                    int memberId = badgeReader.GetInt32("member_id");

                    Badge badge = new Badge();
                    badge.BadgeId = badgeReader.GetInt32("badge_id");
                    badge.Name = badgeReader.GetString("name");
                    badge.Description = badgeReader.IsDBNull(badgeReader.GetOrdinal("description"))
                        ? null
                        : badgeReader.GetString("description");

                    // Find the member this badge belongs to and add it
                    foreach (Member member in memberList)
                    {
                        if (member.MemberId == memberId)
                        {
                            member.Badges.Add(badge);
                            break;
                        }
                    }
                }

                return memberList;   
            }
            
            
            catch (Exception ex)
            {
                throw new Exception("Der opstod en fejl ved hentningen af spejdere ", ex);
            }
            
        }
        
        // Private helper method to assign badges to members
        private void AssignBadgeToMember(Member item)
        {
            List<Badge> allBadges = item.Badges;
            
            try
            {
                using MySqlConnection conn = _db.Connect();
                using MySqlTransaction transaction = conn.BeginTransaction();

                // 1. Remove the member's old badges
                string deleteSql = "DELETE FROM member_badges WHERE member_id = @member_id";
                using MySqlCommand deleteCmd = new MySqlCommand(deleteSql, conn, transaction);
                
                deleteCmd.Parameters.AddWithValue("@member_id", item.MemberId);
                deleteCmd.ExecuteNonQuery();
                

                // 2. Insert the current badges
                string insertSql = "INSERT INTO member_badges (member_id, badge_id) VALUES (@member_id, @badge_id)";
                foreach (Badge badge in allBadges)
                {
                    using MySqlCommand insertCmd = new MySqlCommand(insertSql, conn, transaction);
                    insertCmd.Parameters.AddWithValue("@member_id", item.MemberId);
                    insertCmd.Parameters.AddWithValue("@badge_id", badge.BadgeId);
                    insertCmd.ExecuteNonQuery();
                }

                // 3. Save everything (if anything above failed, nothing is changed)
                transaction.Commit();
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't assign badges to member in db", ex);
            }
        }

        public void AssignActivityToMember(int memberID, int activityId)
        {
            try
            {
                string sql = """
                             INSERT INTO activity_members (activity_id, member_id) VALUES (@activity_id, @member_id)
                             """;

                using MySqlConnection connString = _db.Connect();

                using MySqlCommand cmd = new MySqlCommand(sql, connString);
                cmd.Parameters.AddWithValue("activity_id", activityId);
                cmd.Parameters.AddWithValue("member_id", memberID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong assigning activity to member: ", ex);
            }
        }
        
        
        
        public void UnAssignActivityToMember(int memberID, int activityId)
        {
            try
            {
                string sql = """
                             DELETE FROM activity_members WHERE member_id = @member_id AND activity_id = @activity_id
                             """;

                using MySqlConnection connString = _db.Connect();

                using MySqlCommand cmd = new MySqlCommand(sql, connString);
                cmd.Parameters.AddWithValue("activity_id", activityId);
                cmd.Parameters.AddWithValue("member_id", memberID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong unassigning activity to member: ", ex);
            }
        }


		public Member? GetByUsername(string username)
		{
			const string query = """
            SELECT member_id, first_name, last_name, date_of_birth, phone_number, is_leader, username, password FROM members
            WHERE username = @username
            """;

            using MySqlConnection conn = _db.Connect();

            using MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            using MySqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return new Member(
                memberId: reader.GetInt32("member_id"),
                firstName: reader.GetString("first_name"),
                lastName: reader.GetString("last_name"),
                dateOfBirth: reader.GetDateTime("date_of_birth"),
                phoneNumber: reader.GetString("phone_number"),
                isLeader: reader.GetBoolean("is_leader"),
                username: reader.GetString("username"),
                hashPassword: reader.GetString("password")
            );
		}
	}
}
