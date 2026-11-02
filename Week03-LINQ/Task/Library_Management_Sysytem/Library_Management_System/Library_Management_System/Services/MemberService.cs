using Library_Management_System.Exceptions;
using Library_Management_System.Models;
using Library_Management_System.Repositories;

namespace Library_Management_System.Services
{
    public class MemberService
    {
        private readonly MemberRepository members;

        public MemberService(MemberRepository members)
        {
            this.members = members;
        }

        public List<Member> GetAll() => this.members.GetAll();
        public Member? GetById(int memberId) => this.members.GetById(memberId) ?? throw new LibraryException($"No member exists with ID {memberId}.");

        public Member Add(string fullName, string email)
        {
            var member = new Member { FullName = fullName.Trim(), Email = email.Trim(), IsActive = true };
            Validate(member);
            member.MemberId = members.Add(member);
            return GetById(member.MemberId);
        }

        public Member Update(Member member)
        {
            Member current = GetById(member.MemberId);

            member.FullName = member.FullName.Trim();
            member.Email = member.Email.Trim();
            Validate(member);

            if (current.IsActive && !member.IsActive && current.BooksOnLoan > 0)
                throw new LibraryException(
                    $"{current.FullName} still has {current.BooksOnLoan} book(s) on loan. They must be returned before deactivating.");

            members.Update(member);
            return GetById(member.MemberId);
        }

        public void Delete(int memberId)
        {
            Member member = GetById(memberId);

            if (members.HasLoanHistory(memberId))
                throw new LibraryException(
                    $"{member.FullName} has loan history, so they can't be deleted. ");

            members.Delete(memberId);
        }

        public List<Member> Search(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new LibraryException("Search text cannot be empty.");
            return members.Search(text.Trim());
        }

        //helpers
        private void Validate(Member member)
        {
            if (string.IsNullOrWhiteSpace(member.FullName))
                throw new LibraryException("Name cannot be empty.");
            if (member.FullName.Length > 150)
                throw new LibraryException("Name must be 150 characters or fewer.");
            if (string.IsNullOrWhiteSpace(member.Email))
                throw new LibraryException("Email cannot be empty.");
            if (member.Email.Length > 254)
                throw new LibraryException("Email must be 254 characters or fewer.");
            if (!IsEmailValid(member.Email))
                throw new LibraryException($"\"{member.Email}\" is not a valid email address.");
            IsEmailUnique(member);
        }

        private static bool IsEmailValid(string email)
        {
            int at = email.IndexOf('@');
            return at > 0
                && at == email.LastIndexOf('@')
                && email.IndexOf('.', at) > at + 1
                && !email.EndsWith('.')
                && !email.Contains(' ');
        }

        private void IsEmailUnique(Member member)
        {
            bool taken = members.GetAll().Any(m =>
                m.MemberId != member.MemberId &&
                m.Email.Equals(member.Email, StringComparison.OrdinalIgnoreCase));

            if (taken)
                throw new LibraryException($"The email {member.Email} is already used by another member.");
        }

    }
}
