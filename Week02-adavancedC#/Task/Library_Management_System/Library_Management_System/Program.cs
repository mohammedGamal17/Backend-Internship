using Library_Management_System.Services;
using Library_Management_System.UI;

public static class Program
{
    private static void Main(string[] args)
    {

        var library = new Library();
        library.SeedSampleData();
        var app = new LibraryApp(library);
        app.Run();
    }
}