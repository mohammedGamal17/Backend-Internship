using Library_Management_System.Exceptions;
using Library_Management_System.Models;
using Library_Management_System.Repositories;

namespace Library_Management_System.Services
{

    public enum LoanFilter { All, Open, Overdue, Returned }

    public class LoanService
    {

        public const int LoanPeriodDays = 15;
        public const int MaxBooksPerMember = 5;
        private readonly LoanRepository loans;
        private readonly BookRepository books;
        private readonly MemberRepository members;

        public LoanService(LoanRepository loans, BookRepository books, MemberRepository members)
        {
            this.loans = loans;
            this.books = books;
            this.members = members;
        }

        public List<Loan> GetAll()
        {
            this.loans.RefreshOverdueStatuses();
            return this.loans.GetAll();
        }

        public List<Loan> GetByMember(int memberId)
        {
            if (members.GetById(memberId) is null)
                throw new LibraryException($"No member exists with ID {memberId}.");

            return GetAll().Where(l => l.MemberId == memberId).ToList();
        }

        public List<Loan> GetByBook(int bookId)
        {
            if (books.GetById(bookId) is null)
                throw new LibraryException($"No book exists with ID {bookId}.");
            return GetAll().Where(l => l.Items.Any(i => i.BookId == bookId)).ToList();
        }

        public Loan GetById(int loanId) =>
         GetAll().FirstOrDefault(l => l.LoanId == loanId)
        ?? throw new LibraryException($"No loan exists with ID {loanId}.");

        public List<Loan> Filter(LoanFilter filter) => filter switch
        {
            LoanFilter.Open => GetAll().Where(l => l.Status == "Active").ToList(),
            LoanFilter.Overdue => GetAll().Where(l => l.HasOverdueItems).ToList(),
            LoanFilter.Returned => GetAll().Where(l => l.Status == "Returned").ToList(),
            _ => GetAll(),
        };


        public Loan Borrow(int memberId, List<int> bookIds)
        {
            Member member = CanBorrow(memberId);

            if (bookIds.Count == 0)
                throw new LibraryException("Choose at least one book.");

            if (bookIds.Distinct().Count() != bookIds.Count)
                throw new LibraryException("The same book ID was entered twice. Each book can appear once per loan.");

            List<(Loan Loan, LoanItem Item)> openItems = GetOverdueByMemeber(memberId);

            if (openItems.Count + bookIds.Count > MaxBooksPerMember || member.BooksOnLoan > MaxBooksPerMember)
                throw new LibraryException(
                    $"{member.FullName} has {openItems.Count} book(s) on loan. " +
                    $"Borrowing {bookIds.Count} more would pass the limit of {MaxBooksPerMember}.");

            foreach (int bookId in bookIds)
            {
                Book book = books.GetById(bookId)
                    ?? throw new LibraryException($"No book exists with ID {bookId}.");

                if (!book.CanBeBorrowed)
                    throw new LibraryException($"\"{book.Title}\" can't be borrowed right now: {book.StatusText}.");

                if (openItems.Any(pair => pair.Item.BookId == bookId))
                    throw new LibraryException($"{member.FullName} already has a copy of \"{book.Title}\".");
            }

            DateTime dueDate = DateTime.Today.AddDays(LoanPeriodDays);
            int loanId = loans.CreateLoan(memberId, bookIds, dueDate);
            return GetById(loanId);
        }

        public LoanItem Return(int loanId, int bookId)
        {
            Loan loan = GetById(loanId);
            if (loan.Status == "Returned")
                throw new LibraryException($"Loan {loanId} has already been returned.");
            LoanItem? item = loan.Items.FirstOrDefault(i => i.BookId == bookId);
            if (item is null)
                throw new LibraryException($"Loan {loanId} does not include book ID {bookId}.");
            if (item.IsReturned)
                throw new LibraryException($"Book ID {bookId} has already been returned for loan {loanId}.");
            loans.ReturnLoan(loanId);
            return item;
        }
        // helpers 

        public Member CanBorrow(int memberId)
        {
            Member member = members.GetById(memberId)
            ?? throw new LibraryException($"No member exists with ID {memberId}.");

            if (!member.IsActive)
                throw new LibraryException($"{member.FullName} is inactive and can't borrow books.");

            List<(Loan Loan, LoanItem Item)> openItems = GetOverdueByMemeber(memberId);

            if (openItems.Any(pair => pair.Item.IsOverdue))
                throw new LibraryException($"{member.FullName} has overdue books. They must be returned before borrowing more.");

            if (openItems.Count >= MaxBooksPerMember)
                throw new LibraryException($"{member.FullName} already has {MaxBooksPerMember} books on loan, which is the limit.");

            return member;
        }

        public List<(Loan Loan, LoanItem Item)> GetOverdueByMemeber(int memberId) =>
       GetByMember(memberId)
           .SelectMany(loan => loan.Items.Select(item => (loan, item)))
           .Where(pair => !pair.item.IsReturned)
           .OrderBy(pair => pair.item.DueDate)
           .ToList();


        public List<(Loan Loan, LoanItem Item)> GetAllOverdue() =>
            GetAll()
                .SelectMany(loan => loan.Items.Select(item => (loan, item)))
                .Where(pair => pair.item.IsOverdue)
                .OrderByDescending(pair => pair.item.DaysOverdue)
                .ToList();
    }


}
