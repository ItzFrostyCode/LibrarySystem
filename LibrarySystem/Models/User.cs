using System;

namespace LibrarySystem.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
        public DateTime RegisteredDate { get; set; }
    }
}

