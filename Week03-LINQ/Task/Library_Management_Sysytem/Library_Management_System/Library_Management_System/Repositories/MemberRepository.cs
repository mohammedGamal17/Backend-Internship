using Library_Management_System.Data;
using Library_Management_System.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

namespace Library_Management_System.Repositories
{
    public class MemberRepository
    {
        private readonly DbHelper db;

        private const string selectMembersQuery = @"SELECT  m.MemberId, m.FullName, m.Email, m.JoinDate, m.IsActive,
                (SELECT COUNT(*)
                 FROM   dbo.LoanItems li
                 INNER JOIN dbo.Loans l ON l.LoanId = li.LoanId
                 WHERE  l.MemberId = m.MemberId AND li.ReturnedDate IS NULL) AS BooksOnLoan
        FROM    dbo.Members m";

        public MemberRepository(DbHelper db)
        {
            this.db = db;
        }

        public List<Member> GetAll()
        {
            DataTable table = db.ExecuteQuery(selectMembersQuery + " ORDER BY m.MemberId");

            var members = new List<Member>();
            foreach (DataRow row in table.Rows)
                members.Add(MapRow(row));
            return members;
        }

        public Member? GetById(int memberId)
        {
            DataTable table = db.ExecuteQuery(
            selectMembersQuery + " WHERE m.MemberId = @MemberId",
            new SqlParameter("@MemberId", memberId));

            return table.Rows.Count == 0 ? null : MapRow(table.Rows[0]);

        }

        public int Add(Member member)
        {
            object? newId = db.ExecuteScalar(@"
            INSERT INTO dbo.Members (FullName, Email, JoinDate, IsActive)
            VALUES (@FullName, @Email, CAST(GETDATE() AS DATE), 1);
            SELECT CAST(SCOPE_IDENTITY() AS int);",
            new SqlParameter("@FullName", SqlDbType.NVarChar, 150) { Value = member.FullName },
            new SqlParameter("@Email", SqlDbType.NVarChar, 254) { Value = member.Email });

            return Convert.ToInt32(newId);

        }

        public bool Update(Member member)
        {
            int rows = db.ExecuteNonQuery(@"
            UPDATE dbo.Members
            SET    FullName = @FullName, Email = @Email, IsActive = @IsActive
            WHERE  MemberId = @MemberId;",
            new SqlParameter("@FullName", SqlDbType.NVarChar, 150) { Value = member.FullName },
            new SqlParameter("@Email", SqlDbType.NVarChar, 254) { Value = member.Email },
            new SqlParameter("@IsActive", SqlDbType.Bit) { Value = member.IsActive },
            new SqlParameter("@MemberId", SqlDbType.Int) { Value = member.MemberId });

            return rows > 0;
        }

        public bool Delete(int memberId)
        {

            int rows = db.ExecuteNonQuery(
            "DELETE FROM dbo.Members WHERE MemberId = @MemberId;",
            new SqlParameter("@MemberId", memberId));

            return rows > 0;

        }

        public bool HasLoanHistory(int memberId)
        {
            object? count = db.ExecuteScalar(
                "SELECT COUNT(*) FROM dbo.Loans WHERE MemberId = @MemberId;",
                new SqlParameter("@MemberId", memberId));

            return Convert.ToInt32(count) > 0;
        }

        public bool HasActiveLoans(int memberId)
        {
            object? count = db.ExecuteScalar(
                @"SELECT COUNT(*)
                  FROM dbo.Loans l
                  INNER JOIN dbo.LoanItems li ON li.LoanId = l.LoanId
                  WHERE l.MemberId = @MemberId AND li.ReturnedDate IS NULL;",
                new SqlParameter("@MemberId", memberId));
            return Convert.ToInt32(count) > 0;
        }

        public List<Member> Search(string name)
        {
            DataTable table = db.ExecuteQuery(
                selectMembersQuery + " WHERE m.FullName LIKE @Pattern ORDER BY m.FullName",
                new SqlParameter("@Pattern", "%" + name + "%"));

            var members = new List<Member>();
            foreach (DataRow row in table.Rows)
                members.Add(MapRow(row));
            return members;
        }

        //helpers 
        private static Member MapRow(DataRow row) => new Member
        {
            MemberId = Convert.ToInt32(row["MemberId"]),
            FullName = Convert.ToString(row["FullName"]) ?? "",
            Email = Convert.ToString(row["Email"]) ?? "",
            JoinDate = Convert.ToDateTime(row["JoinDate"]),
            IsActive = Convert.ToBoolean(row["IsActive"]),
            BooksOnLoan = Convert.ToInt32(row["BooksOnLoan"]),
        };

    }
}
