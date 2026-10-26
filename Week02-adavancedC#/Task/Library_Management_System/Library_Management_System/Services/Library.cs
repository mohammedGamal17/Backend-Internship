using Library_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

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

            if (string.IsNullOrWhiteSpace(title))
                throw new InvalidOperationException("Book title cannot be empty.");
            if (string.IsNullOrWhiteSpace(author))
                throw new InvalidOperationException("Author name cannot be empty.");

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
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Member name cannot be empty.");
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("Member email cannot be empty.");
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

            if(!members.ContainsKey(memberId))
            {
                throw new KeyNotFoundException($"Member with Id {memberId} not found.");
            }

            if (!books.ContainsKey(bookId))
            {
                throw new KeyNotFoundException($"Book with Id {bookId} not found.");
            }
           
            book.Borrow(memberId);
            member.BorrowBook(bookId);
            Console.Out.WriteLine($"Book '{book.Title}' borrowed by member '{member.Name}'.");
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
           
            book.Return();
            member.ReturnBook(bookId);
            Console.Out.WriteLine($"Book '{book.Title}' returned by member '{member.Name}'.");
        }
    }
}
