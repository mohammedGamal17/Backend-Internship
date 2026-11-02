using Library_Management_System.Data;
using Library_Management_System.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Library_Management_System.Repositories
{
    public class BookRepository
    {
        private readonly DbHelper db;
        private const string selectBooksQuery = @"
        SELECT  b.BookId, b.Title, b.Author, b.CategoryId, c.CategoryName,
                b.PublishedYear, b.IsAvailable, b.TotalCopies,
                (SELECT COUNT(*) FROM dbo.LoanItems li
                 WHERE li.BookId = b.BookId AND li.ReturnedDate IS NULL) AS BorrowedCopies
        FROM    dbo.Books b
        INNER JOIN dbo.Categories c ON c.CategoryId = b.CategoryId";
        public BookRepository(DbHelper db)
        {
            this.db = db;
        }

        public List<Book> GetAll()
        {
            DataTable table = db.ExecuteQuery(selectBooksQuery + " ORDER BY b.BookId");

            var books = new List<Book>();
            foreach (DataRow row in table.Rows)
                books.Add(MapRow(row));
            return books;
        }

        public Book? GetById(int bookId)
        {
            DataTable table = db.ExecuteQuery(
                selectBooksQuery + " WHERE b.BookId = @BookId",
                new SqlParameter("@BookId", bookId));

            return table.Rows.Count == 0 ? null : MapRow(table.Rows[0]);
        }

        // or do i add each element individually 
        public int Add(Book book)
        {
            object? result = db.ExecuteScalar(
                @"INSERT INTO dbo.Books (Title, Author, CategoryId, PublishedYear, IsAvailable, TotalCopies)
                  VALUES (@Title, @Author, @CategoryId, @PublishedYear, @IsAvailable, @TotalCopies);
                  SELECT SCOPE_IDENTITY();",
                new SqlParameter("@Title", book.Title),
                new SqlParameter("@Author", book.Author),
                new SqlParameter("@CategoryId", book.CategoryId),
                new SqlParameter("@PublishedYear", (object?)book.PublishedYear ?? DBNull.Value),
                new SqlParameter("@IsAvailable", book.IsAvailable),
                new SqlParameter("@TotalCopies", book.TotalCopies));

            return Convert.ToInt32(result);
        }

        public bool Update(Book book)
        {
            int result = db.ExecuteNonQuery(
                @"UPDATE dbo.Books
                  SET Title = @Title,
                      Author = @Author,
                      CategoryId = @CategoryId,
                      PublishedYear = @PublishedYear,
                      IsAvailable = @IsAvailable,
                      TotalCopies = @TotalCopies
                  WHERE BookId = @BookId",
                new SqlParameter("@Title", book.Title),
                new SqlParameter("@Author", book.Author),
                new SqlParameter("@CategoryId", book.CategoryId),
                new SqlParameter("@PublishedYear", (object?)book.PublishedYear ?? DBNull.Value),
                new SqlParameter("@IsAvailable", book.IsAvailable),
                new SqlParameter("@TotalCopies", book.TotalCopies),
                new SqlParameter("@BookId", book.BookId));
            return result > 0;
        }

        public bool Delete(int bookId)
        {
            int result = db.ExecuteNonQuery(
                "DELETE FROM dbo.Books WHERE BookId = @BookId",
                new SqlParameter("@BookId", bookId));
            return result > 0;
        }

        public bool IsBookAvailable(int bookId)
        {
            object? result = db.ExecuteScalar(
                @"SELECT CASE WHEN b.IsAvailable = 1 AND (b.TotalCopies - 
                    (SELECT COUNT(*) FROM dbo.LoanItems li
                     WHERE li.BookId = b.BookId AND li.ReturnedDate IS NULL)) > 0
                  THEN 1 ELSE 0 END
                  FROM dbo.Books b
                  WHERE b.BookId = @BookId",
                new SqlParameter("@BookId", bookId));
            return Convert.ToInt32(result) == 1;
        }


        public List<Book> Search(string text)
        {
            DataTable table = db.ExecuteQuery(
                selectBooksQuery + " WHERE b.Title LIKE @Pattern OR b.Author LIKE @Pattern ORDER BY b.Title",
                new SqlParameter("@Pattern", "%" + text + "%"));

            var books = new List<Book>();
            foreach (DataRow row in table.Rows)
                books.Add(MapRow(row));
            return books;
        }

        //helpers
        private static Book MapRow(DataRow row) => new Book
        {
            BookId = Convert.ToInt32(row["BookId"]),
            Title = Convert.ToString(row["Title"]) ?? "",
            Author = Convert.ToString(row["Author"]) ?? "",
            CategoryId = Convert.ToInt32(row["CategoryId"]),
            CategoryName = Convert.ToString(row["CategoryName"]) ?? "",
            PublishedYear = row["PublishedYear"] is DBNull ? null : Convert.ToInt32(row["PublishedYear"]),
            IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
            TotalCopies = Convert.ToInt32(row["TotalCopies"]),
            BorrowedCopies = Convert.ToInt32(row["BorrowedCopies"]),
        };
    }
}
