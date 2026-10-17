using Payment_Management_System.Interface;
using Payment_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payment_Management_System.Service
{
    internal class PaymentService
    {
        private List<Transaction> transactions = new List<Transaction>();
        private int nextTransactionNumber = 1000;

        public bool ProcessTransaction(IPaymentMethod method, decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Amount must be greater than zero.");
                return false;
            }

            bool success = method.Pay(amount);

            if (!success)
            {
                Console.WriteLine("Payment failed.");
                return false;
            }

            string transactionId =
                $"TXN-{nextTransactionNumber++}";

            var transaction = new Transaction(
                transactionId,
                method.methodName,
                amount
            );
            transactions.Add(transaction);

            Console.WriteLine("Payment completed successfully!");
            Console.WriteLine($"Amount: {amount:F2} EGP");
            Console.WriteLine(
                $"Payment Method: {method.methodName}");
            Console.WriteLine($"Transaction ID: {transactionId}");
            return true;
        }

        public IReadOnlyList<Transaction> GetPaymentHistory()
        {
            return transactions.AsReadOnly();
        }

        public int GetTotalTransactions()
        {
            return transactions.Count;
        }
    }
}
