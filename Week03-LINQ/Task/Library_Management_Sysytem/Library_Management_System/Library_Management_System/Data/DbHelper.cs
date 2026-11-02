using Microsoft.Data.SqlClient;
using System.Data;

namespace Library_Management_System.Data
{
    public class DbHelper
    {
        private readonly string connectionString;

        public DbHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        //for select queries
        public DataTable ExceuteQuery(string query, params SqlParameter[] parameters)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(query, connection);
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.ParameterName, param.Value);
            }
            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }

        //got other type of queries 
        public int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(query, connection);
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.ParameterName, param.Value);
            }
            connection.Open();
            return command.ExecuteNonQuery();
        }
    }
}
