using Library_Management_System.Exceptions;
using Library_Management_System.Models;
using Library_Management_System.Repositories;

namespace Library_Management_System.Services
{
    public class BookService
    {
        private readonly BookRepository books;
        private readonly CategoryRepository categories;

        public BookService(BookRepository books, CategoryRepository categories)
        {
            this.books = books;
            this.categories = categories;
        }

        public List<Book> GetAll() => this.books.GetAll();
        public List<Category> GetCategories() => this.categories.GetAll();
        public Book? GetById(int bookId) => this.books.GetById(bookId) ?? throw new LibraryException($"No book exists with ID {bookId}.");

        public Book Add(string title, string author, int categoryId, int? publishedYear, int totalCopies, bool isAvailable)
        {
            var book = new Book
            {
                Title = title.Trim(),
                Author = author.Trim(),
                CategoryId = categoryId,
                PublishedYear = publishedYear,
                TotalCopies = totalCopies,
                IsAvailable = isAvailable,
            };

            Validate(book);
            book.BookId = books.Add(book);
            return GetById(book.BookId);
        }

        public Book Update(Book book)
        {
            Book current = GetById(book.BookId);  //  checks the book exists

            book.Title = book.Title.Trim();
            book.Author = book.Author.Trim();
            Validate(book);
            if (book.TotalCopies < current.BorrowedCopies)
                throw new LibraryException(
                    $"{current.BorrowedCopies} copies are on loan right now, so total copies can't be lower than that.");

            books.Update(book);
            return GetById(book.BookId);
        }

        public void Delete(int bookId)
        {
            Book book = GetById(bookId);

            if (!books.IsBookAvailable(bookId))
                throw new LibraryException(
                    $"\"{book.Title}\" has loan history, so it can't be deleted. " );

            books.Delete(bookId);
        }

        // helpers
        private void Validate(Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Title))
                throw new LibraryException("Title cannot be empty.");
            if (book == null)
                throw new LibraryException("Book cannot be null.");
            if (string.IsNullOrWhiteSpace(book.Title))
                throw new LibraryException("Book title cannot be empty.");
            if (string.IsNullOrWhiteSpace(book.Author))
                throw new LibraryException("Book author cannot be empty.");
            if (book.TotalCopies < 0)
                throw new LibraryException("Total copies cannot be negative.");
            if (book.PublishedYear.HasValue && book.PublishedYear.Value < 0 || book.PublishedYear.Value > DateTime.Now.Year)
                throw new LibraryException("Published year is invalid");
            

            EnsureCategoryExists(book.CategoryId);
            EnsureNotDuplicate(book);
        }

        private void EnsureCategoryExists(int categoryId)
        {
            if (!categories.GetAll().Any(c => c.CategoryId == categoryId))
                throw new LibraryException($"No category exists with ID {categoryId}.");
        }

        private void EnsureNotDuplicate(Book book)
        {
            bool duplicate = books.GetAll().Any(b =>
                b.BookId != book.BookId &&
                b.Title.Equals(book.Title, StringComparison.OrdinalIgnoreCase) &&
                b.Author.Equals(book.Author, StringComparison.OrdinalIgnoreCase));

            if (duplicate)
                throw new LibraryException($"\"{book.Title}\" by {book.Author} is already in the library.");
        }
    }
}
