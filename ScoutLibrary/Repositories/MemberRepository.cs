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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
