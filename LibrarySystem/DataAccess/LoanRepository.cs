using LibrarySystem.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;

internal class LoanRepository
{
    private const string ConnectionString = "server=127.0.0.1;Database=libsysman;User ID=root;Password=0430;";

    public List<Loan> GetAllLoans()
    {
        var loans = new List<Loan>();
        try
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT LoanID, UserID, BookID, LoanDate, DueDate, ReturnDate, Status FROM Loans", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        loans.Add(new Loan
                        {
                            LoanID = reader.GetInt32("LoanID"),
                            UserID = reader.GetInt32("UserID"),
                            BookID = reader.GetInt32("BookID"),
                            LoanDate = reader.GetDateTime("LoanDate"),
                            DueDate = reader.GetDateTime("DueDate"),
                            ReturnDate = reader.IsDBNull(reader.GetOrdinal("ReturnDate")) ? (DateTime?)null : reader.GetDateTime("ReturnDate"),
                            Status = reader.GetString("Status")
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error loading loans: " + ex.Message, ex);
        }
        return loans;
    }


    public void AddLoan(Loan loan)
    {
        try
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Loans (UserID, BookID, LoanDate, DueDate, Status)
                                        VALUES (@UserID, @BookID, @LoanDate, @DueDate, @Status)";
                    cmd.Parameters.AddWithValue("@UserID", loan.UserID);
                    cmd.Parameters.AddWithValue("@BookID", loan.BookID);
                    cmd.Parameters.AddWithValue("@LoanDate", loan.LoanDate);
                    cmd.Parameters.AddWithValue("@DueDate", loan.DueDate);
                    cmd.Parameters.AddWithValue("@Status", loan.Status);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error adding loan: " + ex.Message, ex);
        }
    }

    public void UpdateLoan(Loan loan)
    {
        try
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Loans SET 
                                            UserID=@UserID, 
                                            BookID=@BookID, 
                                            LoanDate=@LoanDate, 
                                            DueDate=@DueDate, 
                                            ReturnDate=@ReturnDate, 
                                            Status=@Status 
                                        WHERE LoanID=@LoanID";
                    cmd.Parameters.AddWithValue("@UserID", loan.UserID);
                    cmd.Parameters.AddWithValue("@BookID", loan.BookID);
                    cmd.Parameters.AddWithValue("@LoanDate", loan.LoanDate);
                    cmd.Parameters.AddWithValue("@DueDate", loan.DueDate);
                    cmd.Parameters.AddWithValue("@ReturnDate", (object)loan.ReturnDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", loan.Status);
                    cmd.Parameters.AddWithValue("@LoanID", loan.LoanID);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error updating loan: " + ex.Message, ex);
        }
    }
}
