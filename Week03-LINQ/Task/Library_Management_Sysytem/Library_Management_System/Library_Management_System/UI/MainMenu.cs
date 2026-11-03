namespace Library_Management_System.UI
{
    // Access Modifiers
    // Most Common in programming languages are:
    // Public: Accessible from anywhere
    // Private: Accessible only within the same class
    // Protected: Accessible within the same class and derived classes

    // Specific to C#:
    // Internal: Accessible only within the same assembly
    // Partial: Allows a class to be split across multiple files, but all parts must be combined at compile time
    // Protected Internal: Accessible within the same assembly and derived classes
    // Private Protected: Accessible within the same class and derived classes in the same assembly

    public class MainMenu
    {

        public BookMenu BookMenu { get; set; }
        public MemberMenu MemberMenu { get; set; }
        public LoanMenu LoanMenu { get; set; }
        public ReportMenu ReportMenu { get; set; }

        public void Run()               
        {
            var options = new List<(int, string, Action)>
        {
            (1, "Books",   BookMenu.Run),
            (2, "Members", MemberMenu.Run),
            (3, "Loans",   LoanMenu.Run),
            (4, "Reports", ReportMenu.Run),
        };

            // No "Press Enter" pause here: the submenus have their own.
            Helpers.MenuRunner("Library Management System", options, "Exit", pauseAfterEach: false);
            Console.WriteLine("Goodbye!");
        }



    }

}
