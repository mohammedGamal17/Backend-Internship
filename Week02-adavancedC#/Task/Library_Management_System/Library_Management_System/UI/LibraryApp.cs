using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Library_Management_System.Models;
using Library_Management_System.Services;

namespace Library_Management_System.UI
{
    public class LibraryApp
    {
        private readonly Library library;
        private readonly List<(int Number, string Label, Action Handler)> menu;

        public LibraryApp( Library library)
        {
            this.library = library;
            menu = new List<(int, string, Action)>
        {
            (1,  "Display All Books", DisplayAllBooks),
            (2,  "Add New Book", AddNewBook),
            (3,  "Register New Member", RegisterNewMember),
            (4,  "Display All Members", DisplayAllMembers),
            (5,  "Borrow a Book", BorrowBook),
            (6,  "Return a Book", ReturnBook),
            (7,  "Display Available Books", DisplayAvailableBooks),
            (8,  "Display Borrowed Books", DisplayBorrowedBooks),
            (9,  "View Book Details", ViewBookDetails),
            (10, "View Member Details", ViewMemberDetails),
            (11, "Search Books by Title", SearchBooks),
        };

        }


        public void Run()
        {
            while (true)
            {
                ShowMenu();
                int choice = ReadNumber("Choose an option: ");

                if (choice == 0)
                {
                    Console.WriteLine("Exiting the application. Goodbye!");
                    break;
                }

                if (choice < 0 || choice > menu.Count)
                {
                    Console.WriteLine("Invalid choice. Please try again.");
                    continue;
                }
                var selectedOption = menu[choice - 1];
                try
                {
                    selectedOption.Handler.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine();
                Console.Write("Press Enter to continue...");
                Console.ReadLine();
            }

        }


        private void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("================================");
            Console.WriteLine("      LIBRARY MANAGEMENT");
            Console.WriteLine("================================");
            Console.WriteLine($"Available: {library.totalBooks}  Borrowed: {library.totalBorrowedBooks}");
            Console.WriteLine();

            foreach (var option in menu)
                Console.WriteLine($"{option.Number}. {option.Label}");

            Console.WriteLine("0. Exit");
            Console.WriteLine();
        }
        
        //menu methods
        private void DisplayAllBooks()
        {
            Console.WriteLine("All Books in the Library:");
            PrintBooks(library.GetAllBooks());
        }

        private void DisplayAvailableBooks()
        {
            Console.WriteLine("Available Books:");
            PrintBooks(library.GetAvailableBooks());

        }

        private void DisplayBorrowedBooks()
        {

            Console.WriteLine("Borrowed Books:");
            PrintBooks(library.GetBorrowedBooks());

        }

        private void AddNewBook()
        {
            string title = ReadText("Enter book title: ");
            Console.Write("Enter author name: ");
            string author = ReadText("Enter author name: ");
            int publicationYear = ReadNumber("Enter publication year: ");
            if (publicationYear <= 0)
                throw new InvalidOperationException("Publication year must be a positive integer.");
            if (publicationYear > DateTime.Now.Year)
                throw new InvalidOperationException("Publication year cannot be in the future.");
            if (publicationYear < 1400)
                throw new InvalidOperationException("Publication year is too far in the past.");

            try
            {
                Book book = library.AddBook(title, author, publicationYear);
                Console.WriteLine($"Book added successfully with ID {book.Id}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding book: {ex.Message}");
            }



        }

        private void BorrowBook()
        {
            int memberId = ReadNumber("Enter member ID: ");
        
            int bookId = ReadNumber("Enter book ID: ");
            try
            {
                library.BorrowBook(memberId, bookId);
                Console.WriteLine("Book borrowed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error borrowing book: {ex.Message}");
            }


        }

        private void ReturnBook()
        {
            int memberId = ReadNumber("Enter member ID: ");
            int bookId = ReadNumber("Enter book ID: ");
try
            {
                library.ReturnBook(memberId, bookId);
                Console.WriteLine("Book returned successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error returning book: {ex.Message}");
            }
        }

        private void ViewBookDetails()
        {
            int bookId = ReadNumber("Enter book ID: ");
            try
            {
                var book = library.getBookById(bookId);
                Console.WriteLine(book);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving book details: {ex.Message}");
            }

        }

        private void SearchBooks()
        {
            
            string keyword = ReadText("Enter title keyword to search: ");
            var results = library.search(keyword);
            if (results.Any())
            {
                Console.WriteLine("Search Results:");
                PrintBooks(results);
            }
            else
            {
                Console.WriteLine("No books found matching the search criteria.");
            }
        }
        private void RegisterNewMember()
        {
            string name = ReadText("Enter member name: ");
            string email = ReadText("Enter member email: ");
            if (name.GetType() != typeof(string))
                throw new InvalidOperationException("Member name must be a string.");
            if (!email.Contains("@") || !email.Contains("."))
                throw new InvalidOperationException("Invalid email format.");
            try
            {
                library.AddMember(name, email);
                Console.WriteLine("Member registered successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering member: {ex.Message}");
            }
        }

        private void DisplayAllMembers()
        {
            var members = library.GetAllMembers();
            if (!members.Any())
            {
                Console.WriteLine("No members registered in the library.");
                return;
            }
            Console.WriteLine("All Members:");
            foreach (var member in members)
            {
                Console.WriteLine(member);
            }
        }

        private void ViewMemberDetails()
        {
            int memberId = ReadNumber("Enter member ID: ");
            try
            {
                var member = library.getMemberById(memberId);
                Console.WriteLine(member);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving member details: {ex.Message}");
            }
        }


        //helpers
        private void PrintBooks(IEnumerable<Book> books)
        {
            List<Book> list = books.ToList();

            if (list.Count == 0)
            {
                Console.WriteLine("No books to show.");
                return;
            }

            foreach (Book book in list)
                Console.WriteLine(book);
        }

        private int ReadNumber(string message)
        {
            while (true)
            {
                Console.Write(message);
                string input = Console.ReadLine() ?? "";

                if (int.TryParse(input, out int number))
                    return number;

                Console.WriteLine("Please enter a valid number.");
            }
        }

        private string ReadText(string message)
        {
            while (true)
            {
                Console.Write(message);
                string input = (Console.ReadLine() ?? "").Trim();

                if (input != "")
                    return input;

                Console.WriteLine("This field cannot be empty.");
            }
        }

    }
}
