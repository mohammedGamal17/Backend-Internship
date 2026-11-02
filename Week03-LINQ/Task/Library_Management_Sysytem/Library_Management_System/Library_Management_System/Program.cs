using Library_Management_System.Data;
using Library_Management_System.Repositories;
using Library_Management_System.Services;
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

        var bookRepository = new BookRepository(db);
        var categoryRepository = new CategoryRepository(db);
        var memberRepository = new MemberRepository(db);
        var loanRepository = new LoanRepository(db);

        var bookService = new BookService(bookRepository, categoryRepository);
        var memberService = new MemberService(memberRepository);
        var loanService = new LoanService(loanRepository, bookRepository, memberRepository);


    }
}