using System.Data;
using ERManagementSystem.DataAccess;
using Microsoft.Data.SqlClient;

namespace ERManagementSystem.Helpers
{
    public class SqlHelper
    {
        private readonly DatabaseConnection databaseConnection;

        public SqlHelper(DatabaseConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        public SqlDataReader ExecuteReader(string sql, params SqlParameter[] parameters)
        {
            var connection = databaseConnection.Open();
            var command = new SqlCommand(sql, connection);

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }
        // Repositories should call this method in a using statement to ensure proper disposal of the connection and reader
        /* IMPORTANT
           using var reader = _sqlHelper.ExecuteReader(query, parameters);
           while (reader.Read())
            {
                ...
            }
        */
        public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using var connection = databaseConnection.Open();
            using var command = new SqlCommand(sql, connection);

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteNonQuery();
        }
    }
}