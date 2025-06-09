using LibrarySystem.Models;
using MySqlConnector;
using System.Collections.Generic;

namespace LibrarySystem.DataAccess
{
    public class BookRepository
    {
        public List<Book> GetAllBooks()
        {
            var books = new List<Book>();
            using (var conn = DbConnection.GetConnection())
            {
                var cmd = new MySqlCommand("SELECT * FROM Books", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add(new Book
                        {
                            BookID = reader.GetInt32("BookID"),
                            Title = CleanString(reader["Title"]),
                            Author = CleanString(reader["Author"]),
                            ISBN = CleanString(reader["ISBN"]),
                            Publisher = CleanString(reader["Publisher"]),
                            PublicationYear = reader.GetInt32("PublicationYear"),
                            Genre = CleanString(reader["Genre"]),
                            Price = reader.GetDecimal("Price"),
                            QuantityAvailable = reader.GetInt32("QuantityAvailable"),
                            TotalCopies = reader.GetInt32("TotalCopies"),
                            DateAdded = reader.GetDateTime("DateAdded") // <-- Fixed here
                        });
                    }
                }
            }
            return books;
        }


        // Helper method to replace "N/A" or null with empty string
        private string CleanString(object value)
        {
            var str = value?.ToString();
            return string.IsNullOrEmpty(str) || str == "N/A" ? "" : str;
        }


        public void AddBook(Book book)
        {
            using (var conn = DbConnection.GetConnection())
            {
                var cmd = new MySqlCommand(@"INSERT INTO Books 
                    (Title, Author, ISBN, Publisher, PublicationYear, Genre, Price, QuantityAvailable, TotalCopies) 
                    VALUES (@Title, @Author, @ISBN, @Publisher, @PublicationYear, @Genre, @Price, @QuantityAvailable, @TotalCopies)", conn);

                cmd.Parameters.AddWithValue("@Title", book.Title);
                cmd.Parameters.AddWithValue("@Author", book.Author);
                cmd.Parameters.AddWithValue("@ISBN", book.ISBN);
                cmd.Parameters.AddWithValue("@Publisher", book.Publisher);
                cmd.Parameters.AddWithValue("@PublicationYear", book.PublicationYear);
                cmd.Parameters.AddWithValue("@Genre", book.Genre);
                cmd.Parameters.AddWithValue("@Price", book.Price);
                cmd.Parameters.AddWithValue("@QuantityAvailable", book.QuantityAvailable);
                cmd.Parameters.AddWithValue("@TotalCopies", book.TotalCopies);

                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateBook(Book book)
        {
            using (var conn = DbConnection.GetConnection())
            {
                var cmd = new MySqlCommand(@"UPDATE Books SET 
                    Title=@Title, Author=@Author, ISBN=@ISBN, Publisher=@Publisher, 
                    PublicationYear=@PublicationYear, Genre=@Genre, Price=@Price, 
                    QuantityAvailable=@QuantityAvailable, TotalCopies=@TotalCopies 
                    WHERE BookID=@BookID", conn);

                cmd.Parameters.AddWithValue("@Title", book.Title);
                cmd.Parameters.AddWithValue("@Author", book.Author);
                cmd.Parameters.AddWithValue("@ISBN", book.ISBN);
                cmd.Parameters.AddWithValue("@Publisher", book.Publisher);
                cmd.Parameters.AddWithValue("@PublicationYear", book.PublicationYear);
                cmd.Parameters.AddWithValue("@Genre", book.Genre);
                cmd.Parameters.AddWithValue("@Price", book.Price);
                cmd.Parameters.AddWithValue("@QuantityAvailable", book.QuantityAvailable);
                cmd.Parameters.AddWithValue("@TotalCopies", book.TotalCopies);
                cmd.Parameters.AddWithValue("@BookID", book.BookID);

                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteBook(int bookId)
        {
            using (var conn = DbConnection.GetConnection())
            {
                var cmd = new MySqlCommand("DELETE FROM Books WHERE BookID=@BookID", conn);
                cmd.Parameters.AddWithValue("@BookID", bookId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
