using Library_Management_System.Services;

namespace Library_Management_System.UI;


public static class Pager
{
    public const int DefaultPageSize = 5;

    public static void Show<T>(List<T> items, Action<List<T>> printPage, string emptyMessage,
                               int pageSize = DefaultPageSize)
    {
        if (items.Count == 0)
        {
            Console.WriteLine(emptyMessage);
            return;
        }

        int pageCount = PaginationService.PageCount(items.Count, pageSize);
        int page = 1;

        while (true)
        {
            printPage(PaginationService.GetPage(items, page, pageSize));
            Console.WriteLine($"Page {page} of {pageCount}  ({items.Count} in total)");

            if (pageCount == 1) return;

            Console.Write("[N]ext, [P]revious, [Q]uit: ");
            string key = (Console.ReadLine() ?? "q").Trim().ToLower();

            if (key == "n" && page < pageCount) page++;
            else if (key == "p" && page > 1) page--;
            else if (key == "q" || key == "") return;
            Console.WriteLine();
        }
    }

    public static string Fit(string text, int width) =>
        text.Length <= width ? text : text[..(width - 1)] + "…";
}
