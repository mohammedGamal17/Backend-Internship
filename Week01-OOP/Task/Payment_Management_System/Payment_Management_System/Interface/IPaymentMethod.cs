using System;
using System.Collections.Generic;
using System.Text;

namespace Payment_Management_System.Interface
{
    internal interface IPaymentMethod
    {
        public string methodName { get; }
        public bool Pay(decimal amount);
    }
}
