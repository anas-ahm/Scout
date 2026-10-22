using K4os.Compression.LZ4.Internal;

namespace ScoutLibrary.Models
{
    public class Member
    {
        public int MemberId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool IsLeader { get; set; }
        public string Username { get; set; }
        public string HashPassword { get; set; }
        public List<Badge> Badges { get; set; } = new List<Badge>();
        public List<Activity> Activities { get; set; } = new List<Activity>();

        // Constructor
        public Member(int memberId, string firstName, string lastName, string phoneNumber,
            DateTime dateOfBirth, bool isLeader, string username)
        {
            MemberId = memberId;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            DateOfBirth = dateOfBirth;
            IsLeader = isLeader;
            Username = username;
        }

        public Member(int memberId, string firstName, string lastName, string phoneNumber,
            DateTime dateOfBirth, bool isLeader, string username, string hashPassword):
            this(memberId, firstName, lastName, phoneNumber, dateOfBirth, isLeader, username)
        {
            HashPassword = hashPassword;
        }

        // Empty Constructor
        public Member() { }

        public int GetAge()
        {
            DateTime today = DateTime.Today;
            int age = today.Year - DateOfBirth.Year;

            // Subtract one year if the member's birthday hasn't occurred this year yet
            if (DateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}