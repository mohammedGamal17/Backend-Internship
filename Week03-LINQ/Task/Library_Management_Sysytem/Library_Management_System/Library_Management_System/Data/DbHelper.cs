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

        public void TestConnection()
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
        }

        //for select queries
        public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddRange(parameters);
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
            command.Parameters.AddRange(parameters);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        //for queries that return a single value for count and stuff
        public object? ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddRange(parameters);
            connection.Open();
            object? result = command.ExecuteScalar();
            return result;
        }

        // does kaza call at the same time so like loan and loan items should be added at the same time so if one fails the other should also fail
        public void ExecuteInTransaction(Action<SqlCommand> work)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            try
            {
                work(command);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
