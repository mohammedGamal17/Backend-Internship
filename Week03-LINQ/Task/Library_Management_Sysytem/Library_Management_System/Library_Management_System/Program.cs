using Library_Management_System.Data;
using Library_Management_System.Repositories;
using Library_Management_System.Services;
using Library_Management_System.UI;
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
        //if (args.Contains("test"))
        //{
        //    new TestScreen(db).Run();
        //    return;
        //}
        //try
        //{
        //    db.TestConnection();
        //}
        //catch (SqlException ex)
        //{
        //    Console.WriteLine("Could not connect to the database.");
        //    Console.WriteLine(ex.Message);
        //    return;
        //}

        var bookRepository = new BookRepository(db);
        var categoryRepository = new CategoryRepository(db);
        var memberRepository = new MemberRepository(db);
        var loanRepository = new LoanRepository(db);

        var bookService = new BookService(bookRepository, categoryRepository);
        var memberService = new MemberService(memberRepository);
        var loanService = new LoanService(loanRepository, bookRepository, memberRepository);
        var reportService = new ReportService(bookRepository, memberRepository, loanRepository);


        MainMenu mainMenu = GetMainMenu(bookService, memberService, loanService, reportService);
        mainMenu.Run();
    }

    private static MainMenu GetMainMenu(BookService bookService, MemberService memberService, LoanService loanService, ReportService reportService)
    {
        return new MainMenu()
        {
            BookMenu = MapBookMenu(bookService),
            MemberMenu = MapMemberMenu(memberService, loanService),
            LoanMenu = MapLoanMenu(loanService, memberService, bookService),
            ReportMenu = MapReportMenu(reportService, loanService)
        };
    }

    private static MemberMenu MapMemberMenu(MemberService memberService, LoanService loanService)
        => new MemberMenu() { LoanService = loanService, MemberService = memberService };


    private static BookMenu MapBookMenu(BookService bookService)
        => new BookMenu() { BookService = bookService };

    private static ReportMenu MapReportMenu(ReportService reportService, LoanService loanService)
        => new ReportMenu() { ReportService = reportService, LoanService = loanService };

    private static LoanMenu MapLoanMenu(LoanService loanService, MemberService memberService, BookService bookService)
        => new LoanMenu() { LoanService = loanService, MembersService = memberService, BookService = bookService };

}