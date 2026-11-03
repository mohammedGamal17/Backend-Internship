using Library_Management_System.Models;
using Library_Management_System.Services;

namespace Library_Management_System.UI
{
    public class LoanMenu
    {
        public LoanService LoanService { get; set; }
        public MemberService MembersService { get; set; }
        public BookService BookService { get; set; }

       

        public void Run()
        {
            var options = new List<(int, string, Action)>
        {
            (1, "View All Loans",                 ViewAllLoans),
            (2, "Filter Loans by Status",         FilterLoans),
            (3, "Loans by Member",                LoansByMember),
            (4, "View Loan Details",              ViewLoanDetails),
            (5, "Borrow Books",                   BorrowBooks),
            (6, "Return a Book",                  ReturnBook),
            (7, "Overdue Books",                  ShowOverdue),
        };

            Helpers.MenuRunner("Loans Menu", options, "Back to Main Menu");
        }

        private void ViewAllLoans() => ShowPaged(LoanService.GetAll());

        private void FilterLoans()
        {
            Console.WriteLine("1. All   2. Open (books still out)   3. With overdue books   4. Fully returned");
            var filter = (LoanFilter)(Helpers.ReadInt("Show: ", 1, 4) - 1);
            ShowPaged(LoanService.Filter(filter));
        }

        private void LoansByMember()
        {
            Member member = MembersService.GetById(Helpers.ReadId("Member ID"));
            Console.WriteLine($"Loans for {member.FullName}:");
            ShowPaged(LoanService.GetByMember(member.MemberId));
        }

        private void ViewLoanDetails()
        {
            Loan loan = LoanService.GetById(Helpers.ReadId("Loan ID"));
            PrintLoanDetails(loan);
        }

        private void ShowOverdue()
        {
            var overdue = LoanService.GetAllOverdue();
            if (overdue.Count == 0)
            {
                Console.WriteLine("No books are overdue.");
                return;
            }

            Console.WriteLine($"{"Loan",-6}{"Member",-24}{"Book",-32}{"Due",-13}Days late");
            Console.WriteLine(new string('-', 85));
            foreach (var (loan, item) in overdue)
            {
                Console.WriteLine(
                    $"{loan.LoanId,-6}{Pager.Fit(loan.MemberName, 22),-24}{Pager.Fit(item.BookTitle, 30),-32}" +
                    $"{item.DueDate,-13:dd MMM yyyy}{item.DaysOverdue}");
            }
        }


        private void BorrowBooks()
        {
            Member member = MembersService.GetById(Helpers.ReadId("Member ID"));
            LoanService.CanBorrow(member.MemberId);      
            Console.WriteLine($"Member: {member.FullName}, {member.BooksOnLoan} of {LoanService.MaxBooksPerMember} books on loan.");
            Console.WriteLine();

            // Show what can be borrowed right now, to help the user pick IDs.
            var borrowable = BookService.GetByAvailability();
            Console.WriteLine("Books available to borrow:");
            foreach (Book b in borrowable)
                Console.WriteLine($"   {b.BookId,-4} {Pager.Fit(b.Title, 34),-36} {b.StatusText}");
            Console.WriteLine();

            List<int> bookIds = Helpers.ReadIdList("Book ID(s) to borrow, separated by commas");

            Loan loan = LoanService.Borrow(member.MemberId, bookIds);
            Console.WriteLine(
                $"Loan #{loan.LoanId} created for {member.FullName}: {loan.Items.Count} book(s), " +
                $"due back on {loan.Items.First().DueDate:dd MMM yyyy}.");
        }


        private void ReturnBook()
        {
            Member? member = MembersService.GetById(Helpers.ReadId("Member ID returning the book"));
            var openItems = LoanService.GetByMember(member.MemberId);

            if (openItems.Count == 0)
            {
                Console.WriteLine($"{member.FullName} has no books to return.");
                return;
            }

            Console.WriteLine($"Books {member.FullName} still has:");
            Console.WriteLine($"   {"Item",-6}{"Book",-36}Status");
            foreach (Loan loan in openItems)
                foreach (LoanItem item in loan.Items)
                    if (!item.IsReturned)
                        Console.WriteLine($"   {item.LoanItemId,-6}{Pager.Fit(item.BookTitle, 34),-36}{ItemStatus(item)}");

            int itemId = Helpers.ReadId("Item number to return");

            // Only allow returning one of THIS member's books.
            if (!openItems.Any(pair => pair.Items.Any(i => i.LoanItemId == itemId)))
            {
                Console.WriteLine($"Item {itemId} is not one of {member.FullName}'s borrowed books.");
                return;
            }

            LoanItem returned = LoanService.Return(itemId);
            string late = returned.IsOverdue ? $" It was {returned.DaysOverdue} day(s) late." : "";
            Console.WriteLine($"\"{returned.BookTitle}\" was returned.{late}");
        }

       
        public static string ItemStatus(LoanItem item) =>
            item.IsReturned ? $"Returned {item.ReturnedDate:dd MMM yyyy}"
            : item.IsOverdue ? $"OVERDUE by {item.DaysOverdue} day(s), was due {item.DueDate:dd MMM yyyy}"
            : $"Due {item.DueDate:dd MMM yyyy}";

        private static void ShowPaged(List<Loan> loans) => Pager.Show(loans, PrintTable, "No loans to show.");

        private static void PrintTable(List<Loan> loans)
        {
            Console.WriteLine($"{"Loan",-6}{"Member",-24}{"Date",-13}{"Status",-10}{"Books",-7}Still out");
            Console.WriteLine(new string('-', 70));
            foreach (Loan l in loans)
            {
                Console.WriteLine(
                    $"{l.LoanId,-6}{Pager.Fit(l.MemberName, 22),-24}{l.LoanDate,-13:dd MMM yyyy}" +
                    $"{l.Status,-10}{l.Items.Count,-7}{l.ActiveItemCount}");
            }
        }

        private static void PrintLoanDetails(Loan loan)
        {
            Console.WriteLine($"Loan:      #{loan.LoanId}");
            Console.WriteLine($"Member:    {loan.MemberName} (#{loan.MemberId})");
            Console.WriteLine($"Date:      {loan.LoanDate:dd MMM yyyy}");
            Console.WriteLine($"Status:    {loan.Status}");
            Console.WriteLine();
            Console.WriteLine($"   {"Item",-6}{"Book",-36}Status");
            foreach (LoanItem item in loan.Items)
                Console.WriteLine($"   {item.LoanItemId,-6}{Pager.Fit(item.BookTitle, 34),-36}{ItemStatus(item)}");
        }
    }

}
