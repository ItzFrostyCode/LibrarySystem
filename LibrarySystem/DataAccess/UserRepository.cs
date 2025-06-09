using LibrarySystem.Models;
using MySqlConnector;
using System.Collections.Generic;

namespace LibrarySystem.DataAccess
{
    public class UserRepository
    {
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var conn = DbConnection.GetConnection())
            {
                var cmd = new MySqlCommand("SELECT * FROM Users", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            UserID = reader.GetInt32("UserID"),
                            FullName = reader["FullName"].ToString(),
                            ContactNumber = reader["ContactNumber"].ToString(),
                            Email = reader["Email"].ToString(),
                            UserType = reader["UserType"].ToString(),
                            RegisteredDate = reader.GetDateTime("RegisteredDate")
                        });
                    }
                }
            }
            return users;

        }
        public void AddUser(User user)
        {
            using (var conn = DbConnection.GetConnection())
            {
                var cmd = new MySqlCommand(
                    @"INSERT INTO Users (FullName, ContactNumber, Email, UserType) 
              VALUES (@FullName, @ContactNumber, @Email, @UserType)", conn);

                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@ContactNumber", user.ContactNumber);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@UserType", user.UserType);

                cmd.ExecuteNonQuery();
            }
        }



        public void DeleteUser(int userId)
        {
            using (var conn = DbConnection.GetConnection())
            {
                var cmd = new MySqlCommand("DELETE FROM Users WHERE UserID = @UserID", conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateUser(User user)
        {
            using (var conn = DbConnection.GetConnection())
            {
                var cmd = new MySqlCommand(@"UPDATE Users SET 
            FullName = @FullName,
            ContactNumber = @ContactNumber,
            Email = @Email,
            UserType = @UserType
            WHERE UserID = @UserID", conn);

                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@ContactNumber", user.ContactNumber);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@UserType", user.UserType);
                cmd.Parameters.AddWithValue("@UserID", user.UserID);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
