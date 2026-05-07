using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ERManagementSystem.DataAccess
{
    public class DatabaseConnection
    {
        public string ConnectionString { get; }

        public DatabaseConnection(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is missing from appsettings.json.");
        }

        public SqlConnection Open()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public void Close(SqlConnection connection)
        {
            if (connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
            }

            connection.Dispose();
        }
    }
}
