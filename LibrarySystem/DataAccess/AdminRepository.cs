using LibrarySystem.Models;
using MySqlConnector;
using System;

namespace LibrarySystem.DataAccess
{
    public class AdminRepository
    {
        public AdminAccount ValidateLogin(string username, string password)
        {
            using (var conn = DbConnection.GetConnection())
            {
                var cmd = new MySqlCommand("SELECT * FROM Admin_accounts WHERE Username=@Username AND Password=@Password", conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new AdminAccount
                        {
                            AdminID = Convert.ToInt32(reader["AdminID"]),
                            Username = reader["Username"].ToString(),
                            Password = reader["Password"].ToString(),
                            FullName = reader["FullName"].ToString()
                        };
                    }
                }
            }
            return null;
        }
    }
}
