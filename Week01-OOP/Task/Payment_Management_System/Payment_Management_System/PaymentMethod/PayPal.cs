using Payment_Management_System.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payment_Management_System.PaymentMethod
{
    internal class PayPal : IPaymentMethod
    {
        public string methodName => "PayPal";
        public bool Pay(decimal amount)
        {
            Console.WriteLine("Processing PayPal payment...");
            return true;
        }
    }
    
    }

