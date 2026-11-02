using Library_Management_System.Data;
using Library_Management_System.Models;
using System.Data;

namespace Library_Management_System.Repositories
{
    public class CategoryRepository
    {
        private readonly DbHelper db;
        public CategoryRepository(DbHelper db)
        {
            this.db = db;
        }

        public List<Category> GetAll()
        {
            DataTable table = db.ExecuteQuery(
                "SELECT CategoryId, CategoryName FROM dbo.Categories ORDER BY CategoryName;");

            var categories = new List<Category>();
            foreach (DataRow row in table.Rows)
            {
                categories.Add(new Category
                {
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    CategoryName = Convert.ToString(row["CategoryName"]) ?? "",
                });
            }
            return categories;
        }
    }
}
