using Library_Management_System.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library_Management_System.UI
{
    public class ReportMenu
    {
        public int TopCount = 5;
        public ReportService ReportService { get; set; }

        public LoanService LoanService { get; set; }

        
        public void Run()
        {
            var options = new List<(int, string, Action)>
        {
            (1, "Library Summary",                ShowSummary),
            (2, $"Top {TopCount} Most Borrowed Books", ShowMostBorrowed),
            (3, "Overdue Books",                  ShowOverdue),
            (4, $"Top {TopCount} Most Active Members", ShowMostActive),
            (5, "Books per Category",             ShowCategories),
        };

            Helpers.MenuRunner("Reports Menu", options, "Back to Main Menu");
        }

        private void ShowSummary()
        {
            LibrarySummary s = ReportService.GetLibrarySummary();
            Console.WriteLine($"Book titles:         {s.Titles}");
            Console.WriteLine($"Copies owned:        {s.TotalCopies}");
            Console.WriteLine($"Copies on loan:      {s.CopiesOnLoan}");
            Console.WriteLine($"Copies on shelves:   {s.TotalCopies - s.CopiesOnLoan}");
            Console.WriteLine($"Members:             {s.Members} ({s.ActiveMembers} active)");
            Console.WriteLine($"Loans (all time):    {s.Loans} ({s.OpenLoans} still open)");
            Console.WriteLine($"Overdue books:       {s.OverdueBooks}");
        }

        private void ShowMostBorrowed()
        {
            var rows = ReportService.GetBookBorrowCounts(TopCount);
            if (rows.Count == 0) {  Console.WriteLine("No books have been borrowed yet."); return; }

            Console.WriteLine($"{"#",-4}{"ID",-6}{"Title",-36}Times borrowed");
            Console.WriteLine(new string('-', 62));
            int rank = 1;
            foreach (var r in rows)
                Console.WriteLine($"{rank++,-4}{r.BookId,-6}{Pager.Fit(r.Title, 34),-36}{r.TimesBorrowed}");
        }

        private void ShowOverdue()
        {
            var overdue = LoanService.GetAllOverdue();
            if (overdue.Count == 0) { Console.WriteLine("No books are overdue."); return; }

            Console.WriteLine($"{"Loan",-6}{"Member",-24}{"Book",-32}{"Due",-13}Days late");
            Console.WriteLine(new string('-', 85));
            foreach (var (loan, item) in overdue)
            {
                Console.WriteLine(
                    $"{loan.LoanId,-6}{Pager.Fit(loan.MemberName, 22),-24}{Pager.Fit(item.BookTitle, 30),-32}" +
                    $"{item.DueDate,-13:dd MMM yyyy}{item.DaysOverdue}");
            }
            Console.WriteLine($"{overdue.Count} overdue book(s), {overdue.Select(p => p.Loan.MemberId).Distinct().Count()} member(s).");
        }

        private void ShowMostActive()
        {
            var rows = ReportService.GetMemberActivities();
            if (rows.Count == 0) { Console.WriteLine("No loans have been made yet."); return; }

            Console.WriteLine($"{"#",-4}{"ID",-6}{"Member",-26}{"Loans",-8}{"Books",-8}On loan now");
            Console.WriteLine(new string('-', 64));
            int rank = 1;
            foreach (var r in rows)
                Console.WriteLine($"{rank++,-4}{r.MemberId,-6}{Pager.Fit(r.Name, 24),-26}{r.Loans,-8}{r.BooksBorrowed,-8}{r.BooksOnLoanNow}");
        }

        private void ShowCategories()
        {
            var rows = ReportService.GetCategoryStats();
            Console.WriteLine($"{"Category",-16}{"Titles",-9}{"Copies",-9}On loan");
            Console.WriteLine(new string('-', 42));
            foreach (var r in rows)
                Console.WriteLine($"{r.Category,-16}{r.Titles,-9}{r.TotalCopies,-9}{r.CopiesOnLoan}");
        }
    }

}
