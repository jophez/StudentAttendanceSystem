using System.Configuration;
using System.Data.SqlClient;

namespace StudentAttendanceSystem.Data
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public static string GetDefaultConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["_dbConnection"].ToString();
        }
    }
}