namespace Library_Management_System.Models
{
    public class Loan
    {
        public int LoanId { get; set; }
        public int MemberId { get; set; }
        public DateTime LoanDate { get; set; }
        public string Status { get; set; } = "Active";
        public string MemberName { get; set; } = "";
        public List<LoanItem> Items { get; set; } = new List<LoanItem>();
    }
}
