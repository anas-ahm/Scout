using System;
using System.Collections.Generic;
using System.Text;

namespace ScoutLibrary.Models
{
    internal class Member
    {
        // Instance fields
        private string _firstName;
        private string _lastName;
        private string _phoneNumber;
        private DateTime _dateOfBirth;
        private string _username;
        private string _hashPassword;

        // Properties that dont need custom validation
        public int MemberId { get; set; }
        public bool IsLeader { get; set; }
        public List<Badge> Badges { get; set; } = new List<Badge>();
        public List<Activity> Activities { get; set; } = new List<Activity>();
        
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("FirstName can't be empty");
                }
                _firstName = value;
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("LastName can't be empty");
                }
                _lastName = value;
            }
        }

        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("PhoneNumber can't be empty");
                }
                else if (value.Length > 8 || value.Length < 8)
                {
                    throw new ArgumentException("PhoneNumber must be exactly 8 numbers long!");
                }
                _phoneNumber = value;
            }
        }

        public DateTime DateOfBirth
        {
            get { return _dateOfBirth; }
            set
            {
                if (value == default(DateTime) || value > DateTime.Today)
                {
                    throw new ArgumentException("DateOfBirth must be a valid date in the past");
                }
                _dateOfBirth = value;
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Username can't be empty");
                }
                _username = value.Trim();
            }
        }

        public string HashPassword
        {
            get { return _hashPassword; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("HashPassword can't be empty");
                }
                _hashPassword = value;
            }
        }
    }
}
