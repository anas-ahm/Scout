using ScoutLibrary.Models;
using MySqlConnector;

namespace ScoutLibrary.Repositories
{
    public class MemberRepository : ICRUD<Member>, IMemberAuth
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
            
            return member;
        }

        public Member? GetById(int id)
        {
            try
            {
                string sql = "SELECT member_id, first_name, last_name, date_of_birth, phone_number, is_leader, username FROM members WHERE member_id = @member_id";

                using MySqlConnection connString = _db.Connect();

                using MySqlCommand cmd = new MySqlCommand(sql, connString);
                cmd.Parameters.AddWithValue("member_id", id);

                using MySqlDataReader reader = cmd.ExecuteReader();
                
                if (!reader.Read())
                {
                    return null;
                }
                
                Member memberToRead = new Member();
                memberToRead.MemberId = reader.GetInt32("member_id");
                memberToRead.FirstName = reader.GetString("first_name");
                memberToRead.LastName = reader.GetString("last_name");
                memberToRead.DateOfBirth = reader.GetDateTime("date_of_birth");
                memberToRead.PhoneNumber = reader.GetString("phone_number");
                memberToRead.IsLeader = reader.GetBoolean("is_leader");
                memberToRead.Username = reader.GetString("username");
                
                return memberToRead;
            }
            
            // Throws a KeyNotFoundException if member is null 
            catch (Exception ex)
            {
                throw new Exception("Noget gik galt ved at hente spejder: ", ex);
            }
        }

        public List<Member> GetAll()
        {
            try
            {
                string sql = """ SELECT member_id, first_name, last_name, date_of_birth, phone_number, is_leader, username FROM members ORDER BY first_name""";

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

                return memberList;   
            }
            catch (Exception ex)
            {
                throw new Exception("Der opstod en fejl ved hentningen af spejdere ", ex);
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
