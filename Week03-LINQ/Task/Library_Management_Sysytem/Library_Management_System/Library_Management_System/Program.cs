using Library_Management_System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class Program
{
    private static void Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
           .SetBasePath(AppContext.BaseDirectory)
           .AddJsonFile("appsettings.json", optional: false)
           .Build();

        string? connectionString = config.GetConnectionString("LibraryDb");

        var db = new DbHelper(connectionString);

        try
        {
            db.TestConnection();
        }
        catch (SqlException ex)
        {
            Console.WriteLine("Could not connect to the database.");
            Console.WriteLine(ex.Message);
            return;
        }
    }
}