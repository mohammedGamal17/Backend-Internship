namespace Library_Management_System.Services
{
    public class PaginationService
    {
        public static List<T> GetPage<T>(IEnumerable<T> items, int pageNumber, int pageSize) =>
        items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        public static int PageCount(int itemCount, int pageSize) =>
       Math.Max(1, (int)Math.Ceiling(itemCount / (double)pageSize));
    
}
}
