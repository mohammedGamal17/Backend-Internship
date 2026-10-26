using System;
using System.Collections.Generic;
using System.Text;

namespace Library_Management_System.Models
{

    public class Book
    {
        public int Id { get; }
        public string Title { get; }
        public string Author { get; }
        public int PublicationYear { get; }
        public int? BorrowedByMemberId { get; private set; }
        public DateTime? BorrowedOn { get; private set; }
        public bool IsAvailable => BorrowedByMemberId is null;

        public Book(int id, string title, string author, int publicationYear)
        {
            Id = id;
            Title = title;
            Author = author;
            PublicationYear = publicationYear;
        }

        public void Borrow(int memberId)
        {
            if (!IsAvailable)
            {
                throw new InvalidOperationException("Book is already borrowed.");
            }
            BorrowedByMemberId = memberId;
            BorrowedOn = DateTime.Now;
        }

        public void Return()
        {
            if (IsAvailable)
            {
                throw new InvalidOperationException("Book is not borrowed.");
            }
            BorrowedByMemberId = null;
            BorrowedOn = null;
        }

        public override string ToString()
        {
            return $"Book Id: {Id}, Title: {Title}, Author: {Author}, Publication Year: {PublicationYear}, Is Available: {IsAvailable}";
        }
    }
}
