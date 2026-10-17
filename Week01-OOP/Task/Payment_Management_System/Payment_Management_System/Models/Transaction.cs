using System;
using System.Collections.Generic;
using System.Text;

namespace Payment_Management_System.Models
{
    internal class Transaction
    {
        public string transactionId { get; }
        public string paymentMethod { get; }
        public decimal amount { get; }
        public DateTime transactionDate { get; }

        public Transaction(string transactionId, string paymentMethod, decimal amount)
        {
            this.transactionId = transactionId;
            this.paymentMethod = paymentMethod;
            this.amount = amount;
            this.transactionDate = DateTime.Now;
        }

        
    }


}
