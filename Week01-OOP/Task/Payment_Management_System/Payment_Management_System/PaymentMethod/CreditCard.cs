using Payment_Management_System.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payment_Management_System.PaymentMethod
{
    internal class CreditCard : IPaymentMethod
    {
        public string methodName => "Credit Card";
        public bool Pay(decimal amount)
        {
            Console.WriteLine("Processing credit card payment...");
            return true;
        }
    }
}
    

