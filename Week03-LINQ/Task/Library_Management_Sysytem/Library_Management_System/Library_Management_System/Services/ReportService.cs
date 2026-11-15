using Library_Management_System.Models;
using Library_Management_System.Repositories;
namespace Library_Management_System.Services
{

    public record BookBorrowCount(int BookId, string Title, int TimesBorrowed);

    public record MemberActivity(int MemberId, string Name, int Loans, int BooksBorrowed, int BooksOnLoanNow);
    public record CategoryStats(string Category, int Titles, int TotalCopies, int CopiesOnLoan);
    public record LibrarySummary(int Titles, int TotalCopies, int CopiesOnLoan, int Members, int ActiveMembers,
                                 int Loans, int OpenLoans, int OverdueBooks);
    public class ReportService
    {

        private readonly BookRepository books;
        private readonly MemberRepository members;
        private readonly LoanRepository loans;

        public ReportService(BookRepository books, MemberRepository members, LoanRepository loans)
        {
            this.books = books;
            this.members = members;
            this.loans = loans;
        }

        public List<BookBorrowCount> GetBookBorrowCounts(int topCount)
        {
            var allLoans = loans.GetAll();
            var bookCounts = allLoans.SelectMany(l => l.Items)
                                     .GroupBy(i => new { i.BookId, i.BookTitle })
                                     .Select(g => new BookBorrowCount(g.Key.BookId, g.Key.BookTitle, g.Count()))
                                     .OrderByDescending(bc => bc.TimesBorrowed)
                                     .Take(topCount)
                                     .ToList();
            return bookCounts;
        }

        public List<MemberActivity> GetMemberActivities()
        {
            var allLoans = loans.GetAll();
            var memberActivities = allLoans.GroupBy(l => new { l.MemberId, l.MemberName })
                                           .Select(g => new MemberActivity(
                                               g.Key.MemberId,
                                               g.Key.MemberName,
                                               g.Count(),
                                               g.Sum(l => l.Items.Count),
                                               g.Sum(l => l.Items.Count(i => i.ReturnedDate == null))
                                           ))
                                           .OrderByDescending(ma => ma.Loans)
                                           .ToList();
            return memberActivities;
        }

        public List<CategoryStats> GetCategoryStats()
        {
            var allBooks = books.GetAll();
            var categoryStats = allBooks.GroupBy(b => b.CategoryName)
                                        .Select(g => new CategoryStats(
                                            g.Key,
                                            g.Count(),
                                            g.Sum(b => b.TotalCopies),
                                            g.Sum(b => b.BorrowedCopies)
                                        ))
                                        .OrderByDescending(cs => cs.CopiesOnLoan)
                                        .ToList();
            return categoryStats;
        }

        public LibrarySummary GetLibrarySummary()
        {
            var allBooks = books.GetAll();
            var allMembers = members.GetAll();
            var allLoans = loans.GetAll();
            int totalTitles = allBooks.Count;
            int totalCopies = allBooks.Sum(b => b.TotalCopies);
            int copiesOnLoan = allBooks.Sum(b => b.BorrowedCopies);
            int totalMembers = allMembers.Count;
            int activeMembers = allMembers.Count(m => m.IsActive);
            int totalLoans = allLoans.Count;
            int openLoans = allLoans.Count(l => l.Status == "Active");
            int overdueBooks = allLoans.Sum(l => l.Items.Count(i => i.IsOverdue));
            return new LibrarySummary(totalTitles, totalCopies, copiesOnLoan, totalMembers, activeMembers,
                                      totalLoans, openLoans, overdueBooks);
        }

    }
}
