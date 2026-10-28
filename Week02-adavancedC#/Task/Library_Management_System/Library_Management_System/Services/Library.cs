using Library_Management_System.Models;
using System.Text.RegularExpressions;

namespace Library_Management_System.Services
{
    public class Library
    {
        private readonly Dictionary<int, Book> books = new Dictionary<int, Book>();
        private readonly Dictionary<int, Member> members = new Dictionary<int, Member>();
        private int nextBookId = 1;
        private int nextMemberId = 1;


        //books 

        public int totalBooks => books.Count;
        public int totalAvailableBooks => books.Count(b => b.Value.IsAvailable);
        public int totalBorrowedBooks => books.Count(b => !b.Value.IsAvailable);

        public Book AddBook(string title, string author, int publicationYear)
        {

            if (books.Values.Any(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase) && b.Author.Equals(author, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A book with the same title and author already exists.");


            var book = new Book(nextBookId++, title.Trim(), author.Trim(), publicationYear);
            books.Add(book.Id, book);
            Console.Out.WriteLine($"Book added: {book}");
            return book;
        }
        public Book getBookById(int bookId)
        {
            // we use here TryFetValue instead of .containskey since it returns the value if the key exists so it does both checking and returning in one go so its more efficient 

            if (books.TryGetValue(bookId, out var book))
            {
                return book;
            }
            throw new KeyNotFoundException($"Book with Id {bookId} not found.");

        }

        public IEnumerable<Book> GetAllBooks() => books.Values.OrderBy(b => b.Title);
        public IEnumerable<Book> GetAvailableBooks() => GetAllBooks().Where(b => b.IsAvailable);
        public IEnumerable<Book> GetBorrowedBooks() => GetAllBooks().Where(b => !b.IsAvailable);

        public IEnumerable<Book> search(string name)
        {

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Search text cannot be empty or whitespace.");

            return GetAllBooks().Where(b => b.Title.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                                            b.Author.Contains(name, StringComparison.OrdinalIgnoreCase));
        }




        //members 
        public int totalMembers => members.Count;

        public Member AddMember(string name, string email)
        {

            if (members.Values.Any(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A member with the same email already exists.");
            var member = new Member(nextMemberId++, name.Trim(), email.Trim());
            members.Add(member.Id, member);
            Console.Out.WriteLine($"Member added: {member}");

            return member;
        }
        public Member getMemberById(int memberId)
        {
            if (members.TryGetValue(memberId, out var member))
            {
                return member;
            }
            throw new KeyNotFoundException($"Member with Id {memberId} not found.");
        }

        public IEnumerable<Member> GetAllMembers() => members.Values.OrderBy(m => m.Name);


        //actions 

        public void BorrowBook(int memberId, int bookId)
        {
            var member = getMemberById(memberId);
            var book = getBookById(bookId);

            if (!members.ContainsKey(memberId))
            {
                throw new KeyNotFoundException($"Member with Id {memberId} not found.");
            }

            if (!books.ContainsKey(bookId))
            {
                throw new KeyNotFoundException($"Book with Id {bookId} not found.");
            }

            bool isSuccess = member.BorrowBook(bookId);
            if (isSuccess)
            {
                book.Borrow(memberId);
                Console.Out.WriteLine($"Book '{book.Title}' borrowed by member '{member.Name}'.");
            }

        }

        public void ReturnBook(int memberId, int bookId)
        {
            var member = getMemberById(memberId);
            var book = getBookById(bookId);

            if (!members.ContainsKey(memberId))
            {
                throw new KeyNotFoundException($"Member with Id {memberId} not found.");
            }
            if (!books.ContainsKey(bookId))
            {
                throw new KeyNotFoundException($"Book with Id {bookId} not found.");
            }

            bool isSuccess = member.ReturnBook(bookId);
            if (isSuccess)
            {
                book.Return();
                Console.Out.WriteLine($"Book '{book.Title}' returned by member '{member.Name}'.");
            }

        }


        //helpers 

        public bool IsValidMail(string email)
        {
            try
            {
                Regex.IsMatch(email,
                   @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                   RegexOptions.IgnoreCase);
                return true;
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public void SeedSampleData()
        {
            AddBook("Clean Code", "Robert C. Martin", 2008);
            AddBook("The Pragmatic Programmer", "Andrew Hunt", 1999);
            AddBook("C# in Depth", "Jon Skeet", 2019);
            AddBook("Design Patterns", "Erich Gamma", 1994);
            AddBook("Refactoring", "Martin Fowler", 2018);

            AddMember("Ahmed Hassan", "ahmed@example.com");
            AddMember("Sara Mahmoud", "sara@example.com");

            BorrowBook(memberId: 1, bookId: 3);
        }
    }
}
