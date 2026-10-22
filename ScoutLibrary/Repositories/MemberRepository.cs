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
            throw new NotImplementedException();
        }

        public Member Update(Member member, int id)
        {
            throw new NotImplementedException();
        }

        public Member GetById(int id)
        {
            const string query = """
            SELECT member_id, first_name, last_name, date_of_birth, phone_number, is_leader, username, password FROM members
            WHERE member_id = @member_id
            """;

            using MySqlConnection conn = _db.Connect();

            using MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@member_id", id);

            using MySqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                throw new KeyNotFoundException($"Ingen spejder fundet med ID: {id}.");
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

        public List<Member> GetAll()
        {
            throw new NotImplementedException();
        }

		public Member GetByUsername(string username)
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
                throw new KeyNotFoundException($"Ingen spejder fundet med brugernavnet: \"{username}\".");
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
