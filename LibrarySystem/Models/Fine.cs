using System;

namespace LibrarySystem.Models
{
    public class Fine
    {
        public int FineID { get; set; }
        public int LoanID { get; set; }
        public decimal FineAmount { get; set; }
        public DateTime FineDate { get; set; }
        public string Reason { get; set; }
        public string IsPaid { get; set; }
    }


}
