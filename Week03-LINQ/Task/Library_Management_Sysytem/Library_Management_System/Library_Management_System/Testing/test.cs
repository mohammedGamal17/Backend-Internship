using Library_Management_System.Data;
using Library_Management_System.Exceptions;
using Library_Management_System.Models;
using Library_Management_System.Repositories;
using Library_Management_System.Services;
using System.Net.NetworkInformation;
using System.Reflection;

namespace Library_Management_System.Testing;

/// <summary>
/// A developer test screen: calls the repositories and services directly (no menus)
/// and prints PASS / FAIL for each check. Run it from Program.cs while developing.
///
/// Part 1 and 2 only READ data or test rules that refuse before saving, so they change nothing.
/// Part 3 adds a temporary book and member, then deletes them again.
/// </summary>
public class TestScreen
{
    private readonly BookRepository _bookRepo;
    private readonly MemberRepository _memberRepo;
    private readonly LoanRepository _loanRepo;
    private readonly BookService _books;
    private readonly MemberService _members;
    private readonly LoanService _loans;
    private readonly ReportService _reports;

    private int _passed;
    private int _failed;

    public TestScreen(DbHelper db)
    {
        // Build the same objects Program.cs builds for the real app.
        _bookRepo = new BookRepository(db);
        var categoryRepo = new CategoryRepository(db);
        _memberRepo = new MemberRepository(db);
        _loanRepo = new LoanRepository(db);

        _books = new BookService(_bookRepo, categoryRepo);
        _members = new MemberService(_memberRepo);
        _loans = new LoanService(_loanRepo, _bookRepo, _memberRepo);
        _reports = new ReportService(_bookRepo, _memberRepo, _loanRepo);
    }

    public void Run()
    {
        Console.WriteLine("=========================================");
        Console.WriteLine("   TEST SCREEN");
        Console.WriteLine("=========================================");

        ReadingTests();
        RuleTests();

        Console.WriteLine();
        Console.Write("Also run the tests that add and delete a temporary book and member? (y/n): ");
        if ((Console.ReadLine() ?? "").Trim().ToLower() == "y")
            WritingTests();

        Console.WriteLine();
        Console.WriteLine("-----------------------------------------");
        Console.ForegroundColor = _failed == 0 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"   {_passed} passed, {_failed} failed");
        Console.ResetColor();
    }

    // ================================================================ Part 1: reading

    private void ReadingTests()
    {
        Section("Part 1: reading data");

        List<Book> allBooks = _bookRepo.GetAll();
        Info($"Books in the database: {allBooks.Count}");
        Check("Books load", () => allBooks.Count > 0);
        Check("Book 1 is 'Clean Code'", () => _bookRepo.GetById(1)?.Title == "Clean Code");
        Check("Unknown book ID returns null", () => _bookRepo.GetById(999999) is null);
        Check("Every book has a category name", () => allBooks.All(b => b.CategoryName != ""));
        Check("Borrowed copies never exceed total copies", () => allBooks.All(b => b.BorrowedCopies <= b.TotalCopies));

        Check("Categories load", () => _books.GetCategories().Count > 0);

        Check("Search 'clean' finds Clean Code", () => _books.Search("clean").Any(b => b.Title == "Clean Code"));
        Check("Search by author 'orwell' finds 1984", () => _books.Search("orwell").Any(b => b.Title == "1984"));
        Check("Search for nonsense finds nothing", () => _books.Search("zzqqxx").Count == 0);

        List<Book> newest = _books.Sort(allBooks, BookSortOption.NewestFirst);
        Check("Sort newest first: first year >= last year",
              () => (newest.First().PublishedYear ?? 0) >= (newest.Last().PublishedYear ?? 0));

        Check("Paging: page 1 of size 5 has at most 5 books", () => PaginationService.GetPage(allBooks, 1, 5).Count <= 5);
        Check("Paging: 21 items in pages of 10 = 3 pages", () => PaginationService.PageCount(21, 10) == 3);

        List<Member> allMembers = _memberRepo.GetAll();
        Info($"Members in the database: {allMembers.Count}");
        Check("Members load", () => allMembers.Count > 0);
        Check("Search members 'ahmed' finds Ahmed Hassan", () => _members.Search("ahmed").Any(m => m.FullName == "Ahmed Hassan"));
        Check("Filter inactive returns only inactive members",
              () => _members.Filter(MembersFilter.InactiveOnly).All(m => !m.IsActive));

        List<Loan> allLoans = _loans.GetAll();
        Info($"Loans in the database: {allLoans.Count}");
        Check("Loans load", () => allLoans.Count > 0);
        Check("Every loan has at least one item", () => allLoans.All(l => l.Items.Count > 0));
        Check("Every loan has a member name", () => allLoans.All(l => l.MemberName != ""));
        Check("Loans are newest first", () => allLoans.First().LoanDate >= allLoans.Last().LoanDate);
        Check("Overdue list only contains overdue books", () => _loans.GetAllOverdue().All(p => p.Item.IsOverdue));
        Check("No 'Active' loan still has overdue books (status refresh works)",
              () => !allLoans.Any(l => l.Status == "Active" && l.HasOverdueItems));

        LibrarySummary summary = _reports.GetLibrarySummary();
        Check("Report: summary title count matches the books table", () => summary.Titles == allBooks.Count);
        Check("Report: most borrowed books are sorted", () =>
        {
            var top = _reports.GetBookBorrowCounts(5);
            return top.Zip(top.Skip(1)).All(pair => pair.First.TimesBorrowed >= pair.Second.TimesBorrowed);
        });
        Check("Report: books per category adds up to all titles",
              () => _reports.GetCategoryStats().Sum(c => c.Titles) == allBooks.Count);
    }

    // ================================================================ Part 2: rules (nothing is saved)

    private void RuleTests()
    {
        Section("Part 2: rules (each one should be refused)");

        ExpectRefused("Empty search text", () => _books.Search("   "));
        ExpectRefused("Unknown book ID", () => _books.GetById(999999));
        ExpectRefused("Duplicate book (Clean Code by Robert C. Martin)",
                      () => _books.Add("Clean Code", "Robert C. Martin", 1, 2008, 1, true));
        ExpectRefused("Book with a year in the future",
                      () => _books.Add("Future Book", "Someone", 1, DateTime.Today.Year + 1, 1, true));
        ExpectRefused("Book with an unknown category",
                      () => _books.Add("Some Book", "Someone", 999999, 2020, 1, true));
        ExpectRefused("Deleting a book with loan history (book 1)", () => _books.Delete(1));

        ExpectRefused("Member with an invalid email", () => _members.Add("Test Person", "not-an-email"));
        ExpectRefused("Member with an email already in use",
                      () => _members.Add("Test Person", "ahmed.hassan@example.com"));
        ExpectRefused("Deleting a member with loan history (member 1)", () => _members.Delete(1));



        Member? inactive = _memberRepo.GetAll().FirstOrDefault(m => !m.IsActive);
        if (inactive is not null)
            ExpectRefused($"Inactive member ({inactive.FullName}) borrowing", () => _loans.Borrow(inactive.MemberId, new List<int> { 5 }));

        Book? notLendable = _bookRepo.GetAll().FirstOrDefault(b => !b.IsAvailable);
        Member? freeMember = _memberRepo.GetAll().FirstOrDefault(m => m.IsActive && m.BooksOnLoan == 0);
        if (notLendable is not null && freeMember is not null)
            ExpectRefused($"Borrowing a not-lendable book ({notLendable.Title})",
                          () => _loans.Borrow(freeMember.MemberId, new List<int> { notLendable.BookId }));

        if (freeMember is not null)
            ExpectRefused("Borrowing the same book twice in one loan",
                          () => _loans.Borrow(freeMember.MemberId, new List<int> { 5, 5 }));

        ExpectRefused("Returning an unknown loan item", () => _loans.Return(999999));
    }

        // ================================================================ Part 3: writing (cleans up after itself)

    private void WritingTests()
    {
        Section("Part 3: add / update / delete (temporary data)");

        string uniqueTitle = $"Test Book {DateTime.Now:HHmmss}";
        Book? book = null;
        try
        {
            Check("Add a temporary book", () =>
            {
                book = _books.Add(uniqueTitle, "Test Author", 1, 2020, 2, true);
                return book.BookId > 0;
            });
            Check("The new book can be loaded by ID", () => book is not null && _bookRepo.GetById(book.BookId)?.Title == uniqueTitle);
            Check("Update its total copies to 5", () =>
            {
                book!.TotalCopies = 5;
                return _books.Update(book).TotalCopies == 5;
            });
        }
        finally
        {
            if (book is not null)
                Check("Delete the temporary book", () => { _books.Delete(book.BookId); return _bookRepo.GetById(book.BookId) is null; });
        }

        string uniqueEmail = $"test.{DateTime.Now:HHmmss}@example.com";
        Member? member = null;
        try
        {
            Check("Add a temporary member", () =>
            {
                member = _members.Add("Temporary Tester", uniqueEmail);
                return member.MemberId > 0 && member.IsActive;
            });
        }
        finally
        {
            if (member is not null)
                Check("Delete the temporary member", () => { _members.Delete(member.MemberId); return _memberRepo.GetById(member.MemberId) is null; });
        }
    }

    // ================================================================ helpers

    /// <summary>Runs a check that should return true. An exception counts as a failure.</summary>
    private void Check(string name, Func<bool> test)
    {
        try
        {
            if (test()) Pass(name);
            else Fail(name, "returned false");
        }
        catch (Exception ex)
        {
            Fail(name, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>Runs an action that SHOULD be refused with a LibraryException.</summary>
    private void ExpectRefused(string name, Action action)
    {
        try
        {
            action();
            Fail(name, "it was allowed, but it should have been refused");
        }
        catch (LibraryException ex)
        {
            Pass($"{name}  ->  \"{ex.Message}\"");
        }
        catch (Exception ex)
        {
            Fail(name, $"refused with the wrong kind of error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void Pass(string name)
    {
        _passed++;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("  PASS  ");
        Console.ResetColor();
        Console.WriteLine(name);
    }

    private void Fail(string name, string reason)
    {
        _failed++;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("  FAIL  ");
        Console.ResetColor();
        Console.WriteLine($"{name}  ({reason})");
    }

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(title);
        Console.ResetColor();
    }

    private static void Info(string text) => Console.WriteLine($"        {text}");
}