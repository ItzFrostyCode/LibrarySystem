﻿using System;

namespace LibrarySystem.Models
{
    public class Loan
    {
        public int LoanID { get; set; }
        public int UserID { get; set; }
        public int BookID { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } // 'Borrowed', 'Returned', etc.
    }

}
