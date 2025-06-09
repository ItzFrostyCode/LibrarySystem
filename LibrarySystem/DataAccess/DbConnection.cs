using MySqlConnector;

namespace LibrarySystem.DataAccess
{
    public static class DbConnection
    {
        private static readonly string ConnectionString = "Server=127.0.0.1;Database=libsysman;User ID=root;Password=0430;";

        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }
    }
}

