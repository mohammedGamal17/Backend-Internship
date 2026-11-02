namespace Library_Management_System.Models
{
    public class Member
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime JoinDate { get; set; }
        public bool IsActive { get; set; }
        public int Booksloaned { get; set; }
    }
}
