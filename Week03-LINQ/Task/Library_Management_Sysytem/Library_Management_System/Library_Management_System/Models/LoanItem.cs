namespace Library_Management_System.Models
{
    public class LoanItem
    {
        public int LoanItemId { get; set; }
        public int LoanId { get; set; }
        public int BookId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public string BookTitle { get; set; } = "";
        public bool IsReturned => ReturnedDate is not null;
        public bool IsOverdue => !IsReturned && DueDate.Date < DateTime.Today;
        public int DaysOverdue => IsOverdue ? (DateTime.Today - DueDate.Date).Days : 0;
    }

}
