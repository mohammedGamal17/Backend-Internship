using Library_Management_System.Models;
using Library_Management_System.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Library_Management_System.UI
{
    public class MemberMenu
    {
        public  MemberService MemberService { get; set; }
        public  LoanService LoanService{ get; set; }

        public void Run()
        {
            var options = new List<(int, string, Action)>
        {
            (1, "View All Members",ViewAllMembers),
            (2, "Search by Name or Email", SearchMembers),
            (3, "Filter Active / Inactive", FilterMembers),
            (4, "Sort Members", SortMembers),
            (5, "View Member Details", ViewMemberDetails),
            (6, "Add a Member",AddMember),
            (7, "Update a Member",UpdateMember),
            (8, "Delete a Member", DeleteMember),
        };

            Helpers.MenuRunner("Members Menu", options, "Back to Main Menu");
        }


        private void SearchMembers()
        {
            string text = Helpers.ReadRequiredText("Name or email contains");
            List<Member> results = MemberService.Search(text);
            Console.WriteLine($"{results.Count} member(s) found.");
            ShowPaged(results);
        }

        public void ViewAllMembers()
        {
            List<Member> members = MemberService.GetAll();
            PrintTable(members);
        }

        private void FilterMembers()
        {
            Console.WriteLine("1. All   2. Active only   3. Inactive only");
            var filter = (MembersFilter)(Helpers.ReadInt("Filter by: ", 1, 3) - 1);
            ShowPaged(MemberService.Filter(filter));
        }

        private void SortMembers()
        {
            Console.WriteLine("1. Name   2. Newest member first   3. Most books on loan");
            var sortBy = (MemberSortOption)(Helpers.ReadInt("Sort by: ", 1, 3) - 1);
            ShowPaged(MemberService.Sort(MemberService.GetAll(), sortBy));
        }

        private void ViewMemberDetails()
        {
            Member member = MemberService.GetById(Helpers.ReadId("Member ID"));

            Console.WriteLine($"ID:              {member.MemberId}");
            Console.WriteLine($"Name:            {member.FullName}");
            Console.WriteLine($"Email:           {member.Email}");
            Console.WriteLine($"Joined:          {member.JoinDate:dd MMM yyyy}");
            Console.WriteLine($"Status:          {(member.IsActive ? "Active" : "Inactive")}");
            Console.WriteLine($"Books on loan:   {member.BooksOnLoan} of {LoanService.MaxBooksPerMember} allowed");

            List<Loan> loans = LoanService.GetByMember(member.MemberId);
            Console.WriteLine();
            if (loans.Count == 0)
            {
                Console.WriteLine("This member has never borrowed a book.");
                return;
            }

            Console.WriteLine("Loan history (newest first):");
            foreach (Loan loan in loans)
            {
                Console.WriteLine($"  Loan #{loan.LoanId}  {loan.LoanDate:dd MMM yyyy}  {loan.Status}");
                foreach (LoanItem item in loan.Items)
                {
                    Console.WriteLine($"    {item.BookTitle}  {item.LoanDate:dd MMM yyyy}  {(item.ReturnedDate is null ? "Not returned" : item.ReturnedDate.Value.ToString("dd MMM yyyy"))}");
                }
            }
        }

        private void AddMember()
        {
            string name = Helpers.ReadRequiredText("Full name", 150);
            string email = Helpers.ReadRequiredText("Email", 254);

            Member member = MemberService.Add(name, email);
            Console.WriteLine($"{member.FullName} was registered with member ID {member.MemberId}.");
        }

        private void UpdateMember()
        {
            Member member = MemberService.GetById(Helpers.ReadId("Member ID to update"));
            Console.WriteLine($"Editing #{member.MemberId} {member.FullName}. Press Enter on any field to keep its current value.");
            Console.WriteLine();

            string name = Helpers.ReadOptionalText($"Full name [{member.FullName}]");
            if (name.Length > 0) member.FullName = name;

            string email = Helpers.ReadOptionalText($"Email [{member.Email}]");
            if (email.Length > 0) member.Email = email;

            bool? active = Helpers.ReadOptionalYesNo($"Active [{(member.IsActive ? "y" : "n")}]");
            if (active is not null) member.IsActive = active.Value;

            Member saved = MemberService.Update(member);
            Console.WriteLine($"Member #{saved.MemberId} was updated.");
        }

        private void DeleteMember()
        {
            Member member = MemberService.GetById(Helpers.ReadId("Member ID to delete"));

            if (!Helpers.ReadYesNo($"Delete {member.FullName} ({member.Email})? This can't be undone."))
            {
                Console.WriteLine("Nothing was deleted.");
                return;
            }

            MemberService.Delete(member.MemberId);
            Console.WriteLine($"{member.FullName} was deleted.");
        }

        // helpers 
        private static void ShowPaged(List<Member> members) => Pager.Show(members, PrintTable, "No members to show.");

        private static void PrintTable(List<Member> members)
        {
            Console.WriteLine($"{"ID",-5}{"Name",-20}{"Email",-30}{"Status",-15}");
            Console.WriteLine(new string('-', 100));
            foreach (Member m in members)
            {
                Console.WriteLine(
                    $"{m.MemberId,-5}{Pager.Fit(m.FullName, 20),-22}{Pager.Fit(m.Email, 30),-32}{m.IsActive,-17}");
            }
        }

    }
}
