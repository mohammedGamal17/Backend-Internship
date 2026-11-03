using Library_Management_System.Data;
using Library_Management_System.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Library_Management_System.Repositories
{
    public class LoanRepository
    {
        private readonly DbHelper db;
        public LoanRepository(DbHelper db)
        {
            this.db = db;
        }

        public List<Loan> GetAll()
        {
            DataTable table = db.ExecuteQuery(@"
        SELECT  l.LoanId, l.MemberId, m.FullName, l.LoanDate, l.Status,
                li.LoanItemId, li.BookId, b.Title, li.DueDate, li.ReturnedDate
        FROM    dbo.Loans l
        INNER JOIN dbo.Members   m  ON m.MemberId = l.MemberId
        LEFT  JOIN dbo.LoanItems li ON li.LoanId  = l.LoanId
        LEFT  JOIN dbo.Books     b  ON b.BookId   = li.BookId
        ORDER BY l.LoanDate DESC, l.LoanId DESC, li.LoanItemId;");

            var loans = new List<Loan>();
            var loansById = new Dictionary<int, Loan>();

            foreach (DataRow row in table.Rows)
            {
                int loanId = Convert.ToInt32(row["LoanId"]);

                if (!loansById.TryGetValue(loanId, out Loan? loan))
                {
                    loan = new Loan
                    {
                        LoanId = loanId,
                        MemberId = Convert.ToInt32(row["MemberId"]),
                        MemberName = Convert.ToString(row["FullName"]) ?? "",
                        LoanDate = Convert.ToDateTime(row["LoanDate"]),
                        Status = Convert.ToString(row["Status"]) ?? "",
                    };
                    loansById[loanId] = loan;
                    loans.Add(loan);
                }

                if (row["LoanItemId"] is DBNull) continue;

                loan.Items.Add(MapItem(row));
            }

            return loans;
        }

        public int CreateLoan(int memberId, IEnumerable<int> bookIds, DateTime dueDate)
        {
            int loanId = 0;

            db.ExecuteInTransaction(command =>
            {
                command.CommandText = @"
                INSERT INTO dbo.Loans (MemberId, LoanDate, Status)
                VALUES (@MemberId, CAST(GETDATE() AS DATE), 'Active');
                SELECT CAST(SCOPE_IDENTITY() AS int);";
                command.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.Int) { Value = memberId });
                loanId = Convert.ToInt32(command.ExecuteScalar());

                command.CommandText = @"
                INSERT INTO dbo.LoanItems (LoanId, BookId, DueDate, ReturnedDate)
                VALUES (@LoanId, @BookId, @DueDate, NULL);";

                foreach (int bookId in bookIds)
                {
                    command.Parameters.Clear();
                    command.Parameters.Add(new SqlParameter("@LoanId", SqlDbType.Int) { Value = loanId });
                    command.Parameters.Add(new SqlParameter("@BookId", SqlDbType.Int) { Value = bookId });
                    command.Parameters.Add(new SqlParameter("@DueDate", SqlDbType.Date) { Value = dueDate.Date });
                    command.ExecuteNonQuery();
                }
            });

            return loanId;
        }

        public void ReturnLoan(int loanItemId)
        {
            db.ExecuteInTransaction(command =>
            {
                command.CommandText = @"
                UPDATE dbo.LoanItems
                SET    ReturnedDate = CAST(GETDATE() AS DATE)
                WHERE  LoanItemId = @LoanItemId AND ReturnedDate IS NULL;

                DECLARE @LoanId INT = (SELECT LoanId FROM dbo.LoanItems WHERE LoanItemId = @LoanItemId);

                UPDATE dbo.Loans
                SET Status = CASE
                    WHEN NOT EXISTS (SELECT 1 FROM dbo.LoanItems
                                     WHERE LoanId = @LoanId AND ReturnedDate IS NULL)
                        THEN 'Returned'
                    WHEN EXISTS (SELECT 1 FROM dbo.LoanItems
                                 WHERE LoanId = @LoanId AND ReturnedDate IS NULL
                                   AND DueDate < CAST(GETDATE() AS DATE))
                        THEN 'Overdue'
                    ELSE 'Active'
                END
                WHERE LoanId = @LoanId;";
                command.Parameters.Add(new SqlParameter("@LoanItemId", SqlDbType.Int) { Value = loanItemId });
                command.ExecuteNonQuery();
            });
        }

        public void RefreshOverdueStatuses()
        {
            db.ExecuteNonQuery(@"
        UPDATE l
        SET    l.Status = 'Overdue'
        FROM   dbo.Loans l
        WHERE  l.Status = 'Active'
          AND  EXISTS (SELECT 1 FROM dbo.LoanItems li
                       WHERE li.LoanId = l.LoanId
                         AND li.ReturnedDate IS NULL
                         AND li.DueDate < CAST(GETDATE() AS DATE));");
        }
        //helpers 
        private static LoanItem MapItem(DataRow row) => new LoanItem
        {
            LoanItemId = Convert.ToInt32(row["LoanItemId"]),
            LoanId = Convert.ToInt32(row["LoanId"]),
            BookId = Convert.ToInt32(row["BookId"]),
            BookTitle = Convert.ToString(row["Title"]) ?? "",
            DueDate = Convert.ToDateTime(row["DueDate"]),
            ReturnedDate = row["ReturnedDate"] is DBNull ? null : Convert.ToDateTime(row["ReturnedDate"]),
        };
    }
}
