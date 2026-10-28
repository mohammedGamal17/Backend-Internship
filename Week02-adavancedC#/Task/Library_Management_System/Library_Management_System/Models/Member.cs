namespace Library_Management_System.Models
{
    public class Member
    {
        public int Id { get; }
        public string Name { get; }
        public string Email { get; }
        public DateTime RegisteredOn { get; }
        public const int maxBooks = 5;
        private readonly List<int> borrowedBooks = new List<int>();
        public IReadOnlyList<int> BorrowedBookIds => borrowedBooks;

        public Member(int id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
            RegisteredOn = DateTime.Now;
        }

        public bool BorrowBook(int bookId)
        {
            bool x;
            if (borrowedBooks.Count >= maxBooks)
            {

                throw new InvalidOperationException("Member has reached the maximum number of borrowed books.");

            }
            x = true;
            borrowedBooks.Add(bookId);
            return x;

        }

        public bool ReturnBook(int bookId)
        {
            bool x;
            if (!borrowedBooks.Contains(bookId))
            {
                throw new InvalidOperationException("This book was not borrowed by the member.");
            }
            x = true;
            borrowedBooks.Remove(bookId);
            return x;

        }

        public bool HasBorrowedBook(int bookId)
        {
            return borrowedBooks.Contains(bookId);
        }

        public override string ToString()
        {
            return $"Member Id: {Id}, Name: {Name}, Email: {Email}, Registered On: {RegisteredOn}, Borrowed Books Count: {borrowedBooks.Count}";
        }



    }
}
