using System;

namespace LibrarySystem.Models
{
    public class Book
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Publisher { get; set; }
        public int PublicationYear { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
        public int TotalCopies { get; set; }
        public DateTime DateAdded { get; set; } // <-- Use DateAdded, not Date
    }

}
