using Library_Management_System.Models;
using Library_Management_System.Services;

namespace Library_Management_System.UI
{
    public class BookMenu
    {
        public BookService BookService { get; set; }


        public void Run()
        {
            var options = new List<(int, string, Action)>
        {
            (1, "View All Books",ViewAllBooks),
            (2, "Search by Title or Author",SearchBooks),
            (3, "Filter by Category",FilterByCategory),
            (4, "Sort Books",SortBooks),
            (5, "Show Books You Can Borrow", ShowBorrowable),
            (6, "View Book Details", ViewBookDetails),
            (7, "Add a Book", AddBook),
            (8, "Update a Book",UpdateBook),
            (9, "Delete a Book", DeleteBook),
        };

            Helpers.MenuRunner("Books Menu", options, "Back to Main Menu");
        }


        private void ViewAllBooks() => ShowPaged(BookService.GetAll());
        private void SearchBooks()
        {
            string text = Helpers.ReadRequiredText("Title or author contains");
            List<Book> results = BookService.Search(text);
            Console.WriteLine($"{results.Count} book(s) found.");
            ShowPaged(results);
        }

        private void FilterByCategory()
        {
            int categoryId = PickCategory();
            ShowPaged(BookService.GetByCategory(categoryId));
        }

        private void SortBooks()
        {
            Console.WriteLine("1. Title   2. Author   3. Newest first   4. Category   5. Most copies available");
            int choice = Helpers.ReadInt("Sort by: ", 1, 5);

            var sortBy = (BookSortOption)(choice - 1);
            ShowPaged(BookService.Sort(BookService.GetAll(), sortBy));
        }

        private void ShowBorrowable() => ShowPaged(BookService.GetByAvailability());

        private void ViewBookDetails()
        {
            int bookId = Helpers.ReadId("Book ID");
            Book book = BookService.GetById(bookId);
            Console.WriteLine($"ID:              {book.BookId}");
            Console.WriteLine($"Title:           {book.Title}");
            Console.WriteLine($"Author:          {book.Author}");
            Console.WriteLine($"Category:        {book.CategoryName}");
            Console.WriteLine($"Published:       {book.PublishedYear?.ToString() ?? "-"}");
            Console.WriteLine($"Copies:          {book.TotalCopies} total, {book.BorrowedCopies} on loan, {book.AvailableCopies} on the shelf");
            Console.WriteLine($"Lendable:        {(book.IsAvailable ? "Yes" : "No")}");
            Console.WriteLine($"Status:          {book.StatusText}");
        }

        private void AddBook()
        {
            string title = Helpers.ReadRequiredText("Title", 200);
            string author = Helpers.ReadRequiredText("Author", 150);
            int categoryId = PickCategory();
            int year = Helpers.ReadInt($"Published year (1450-{DateTime.Today.Year}): ", 1450, DateTime.Today.Year);
            int copies = Helpers.ReadInt("Total copies (0-1000): ", 0, 1000);

            Book book = BookService.Add(title, author, categoryId, year, copies, isAvailable: true);
            Console.WriteLine($"\"{book.Title}\" was added with ID {book.BookId}.");
        }

        private void UpdateBook()
        {
            Book book = BookService.GetById(Helpers.ReadId("Book ID to update"));
            Console.WriteLine($"Editing #{book.BookId} \"{book.Title}\". Press Enter on any field to keep its current value.");
            Console.WriteLine();

            string title = Helpers.ReadOptionalText($"Title [{book.Title}]");
            if (title.Length > 0) book.Title = title;

            string author = Helpers.ReadOptionalText($"Author [{book.Author}]");
            if (author.Length > 0) book.Author = author;

            if (Helpers.ReadOptionalYesNo($"Change category [{book.CategoryName}]?") == true)
                book.CategoryId = PickCategory();

            int? year = Helpers.ReadOptionalInt($"Published year [{book.PublishedYear?.ToString() ?? "-"}]", 1450, DateTime.Today.Year);
            if (year is not null) book.PublishedYear = year;

            int? copies = Helpers.ReadOptionalInt($"Total copies [{book.TotalCopies}]", 0, 1000);
            if (copies is not null) book.TotalCopies = copies.Value;

            bool? lendable = Helpers.ReadOptionalYesNo($"Lendable [{(book.IsAvailable ? "y" : "n")}]");
            if (lendable is not null) book.IsAvailable = lendable.Value;

            Book saved = BookService.Update(book);
            Console.WriteLine($"Book #{saved.BookId} was updated.");
        }

        private void DeleteBook()
        {
            Book book = BookService.GetById(Helpers.ReadId("Book ID to delete"));

            if ((bool)!Helpers.ReadOptionalYesNo($"Delete \"{book.Title}\" by {book.Author}? This can't be undone."))
            {
                Console.WriteLine("Nothing was deleted.");
                return;
            }

            BookService.Delete(book.BookId);
            Console.WriteLine($"\"{book.Title}\" was deleted.");
        }

        // helpers 
        private int PickCategory()
        {
            foreach (Category c in BookService.GetCategories())
                Console.WriteLine($"   {c.CategoryId}. {c.CategoryName}");
            return Helpers.ReadId("Category ID");
        }

        private static void ShowPaged(List<Book> books) => Pager.Show(books, PrintTable, "No books to show.");

        private static void PrintTable(List<Book> books)
        {
            Console.WriteLine($"{"ID",-5}{"Title",-32}{"Author",-24}{"Category",-14}{"Year",-6}Status");
            Console.WriteLine(new string('-', 100));
            foreach (Book b in books)
            {
                Console.WriteLine(
                    $"{b.BookId,-5}{Pager.Fit(b.Title, 30),-32}{Pager.Fit(b.Author, 22),-24}{Pager.Fit(b.CategoryName, 12),-14}" +
                    $"{b.PublishedYear?.ToString() ?? "-",-6}{b.StatusText}");
            }
        }
    }
}
