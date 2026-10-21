using ScoutLibrary.Models;
using MySqlConnector;

namespace ScoutLibrary.Repositories
{
    public class MemberRepository : ICRUD<Member>
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
            throw new NotImplementedException();
        }

        public Member GetById(int id)
        {
            throw new NotImplementedException();
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
    }
}
