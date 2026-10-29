
using Microsoft.Data.SqlClient;

namespace ElegantEventsWeb.Data
{
    public class DatabaseConnection
    {
        private readonly IConfiguration _configuration;

        public DatabaseConnection(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection GetConnection()
        {
            string connectionString =
                _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Database connection string is missing.");

            return new SqlConnection(connectionString);
        }
    }
}
