using Library_Management_System.Models;
using Library_Management_System.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library_Management_System.Services
{
    public class LoanService
    {

        public const int LoanPeriodDays = 15;
        public const int MaxBooksPerMember = 5;
        private readonly LoanRepository loans;
        private readonly BookRepository books;
        private readonly MemberRepository members;

        public LoanService(LoanRepository loans, BookRepository books, MemberRepository members)
        {
            this.loans = loans;
            this.books = books;
            this.members = members;
        }

        public List<Loan> GetAll()
        {
            this.loans.RefreshOverdueStatuses();
            return this.loans.GetAll();
        }
    }
}
