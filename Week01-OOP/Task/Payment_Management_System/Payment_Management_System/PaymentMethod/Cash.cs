using Payment_Management_System.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payment_Management_System.PaymentMethod
{
    internal class Cash : IPaymentMethod
    {
        public string methodName => "Cash";
        public bool Pay(decimal amount)
        {
            Console.WriteLine("Processing cash payment...");
            return true;
        }
    }
    
    }

