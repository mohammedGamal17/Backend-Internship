namespace Library_Management_System.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public int CategoryId { get; set; }
        public int? PublishedYear { get; set; }
        public bool IsAvailable { get; set; }
        public int TotalCopies { get; set; }
        public string CategoryName { get; set; } = "";
        public int BorrowedCopies { get; set; }
        public int AvailableCopies => Math.Max(0, TotalCopies - BorrowedCopies);
        public bool CanBeBorrowed => IsAvailable && AvailableCopies > 0;

        public string StatusText =>
            !IsAvailable ? "Can not be borrowed"
            : AvailableCopies > 0 ? $"Available ({AvailableCopies}/{TotalCopies})"
            : "All copies out";
    }

}
