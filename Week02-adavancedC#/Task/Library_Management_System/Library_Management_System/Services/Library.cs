using Library_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library_Management_System.Services
{
    public class Library
    {
        private readonly Dictionary<int, Book> books = new();
        private readonly Dictionary<int, Member> members = new();
        private int _nextBookId = 1;
        private int _nextMemberId = 1;

       

    }
}
