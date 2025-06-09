using LibrarySystem.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace LibrarySystem.DataAccess
{
    public class FineRepository
    {
        private const string ConnectionString = "server=127.0.0.1;Database=libsysman;User ID=root;Password=0430;";

        public List<Fine> GetAllFines()
        {
            var fines = new List<Fine>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT * FROM Fine", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        fines.Add(new Fine
                        {
                            FineID = reader.GetInt32("FineID"),
                            LoanID = reader.GetInt32("LoanID"),
                            FineAmount = reader.GetDecimal("FineAmount"),
                            FineDate = reader.GetDateTime("FineDate"),
                            Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? "" : reader.GetString("Reason"),
                            IsPaid = reader.IsDBNull(reader.GetOrdinal("IsPaid")) ? "No" : reader.GetString("IsPaid")
                        });
                    }
                }
            }
            return fines;
        }

        public void AddFine(Fine fine)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    "INSERT INTO Fine (LoanID, FineAmount, FineDate, Reason, IsPaid) VALUES (@LoanID, @FineAmount, @FineDate, @Reason, @IsPaid)", conn))
                {
                    cmd.Parameters.AddWithValue("@LoanID", fine.LoanID);
                    cmd.Parameters.AddWithValue("@FineAmount", fine.FineAmount);
                    cmd.Parameters.AddWithValue("@FineDate", fine.FineDate == default(DateTime) ? DateTime.Now : fine.FineDate);
                    cmd.Parameters.AddWithValue("@Reason", string.IsNullOrEmpty(fine.Reason) ? (object)DBNull.Value : fine.Reason);
                    cmd.Parameters.AddWithValue("@IsPaid", string.IsNullOrEmpty(fine.IsPaid) ? "No" : fine.IsPaid);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (MySqlException ex)
                    {
                        throw new Exception("Error inserting fine: " + ex.Message, ex);
                    }
                }
            }
        }

        public void UpdateFine(Fine fine)
        {
            using (var conn = new MySqlConnector.MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new MySqlConnector.MySqlCommand(
                    "UPDATE Fine SET IsPaid=@IsPaid WHERE FineID=@FineID", conn))
                {
                    cmd.Parameters.AddWithValue("@IsPaid", fine.IsPaid);
                    cmd.Parameters.AddWithValue("@FineID", fine.FineID);
                    cmd.ExecuteNonQuery();
                }
            }
        }


    }
}
