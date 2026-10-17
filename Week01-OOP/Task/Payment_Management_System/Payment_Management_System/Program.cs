using Payment_Management_System.Interface;
using Payment_Management_System.PaymentMethod;
using Payment_Management_System.Service;

internal class Program
{

    private static readonly PaymentService paymentService = new();

    private static void Main(string[] args)
    {
        Run();


    }

    public static void Run()
    {
        bool isRunning = true;

        while (isRunning)
        {
            ShowMenu();

            Console.Write("Choose an option: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    MakePayment();
                    break;

                case "2":
                    ViewPaymentHistory();
                    break;

                case "3":
                    isRunning = false;
                    Console.WriteLine("Goodbye!");
                    break;

                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
        }
    }


    static void ShowMenu()
    {
        Console.WriteLine("\n====================================");
        Console.WriteLine("     PAYMENT MANAGEMENT SYSTEM");
        Console.WriteLine("====================================");
        Console.WriteLine("1. Make a Payment");
        Console.WriteLine("2. View Payment History");
        Console.WriteLine("3. Exit");
    }


    static void MakePayment()
    {
        Console.WriteLine("\n----------- MAKE A PAYMENT -----------");
        Console.WriteLine("1. Credit Card");
        Console.WriteLine("2. PayPal");
        Console.WriteLine("3. Cash");

        Console.Write("Choose payment method: ");
        string? choice = Console.ReadLine();

        

        IPaymentMethod? paymentMethod = choice switch
        {
            "1" => new CreditCard(),
            "2" => new PayPal(),
            "3" => new Cash(),
            _ => null
        };

        if (paymentMethod == null)
        {
            Console.WriteLine("Invalid payment method.");
            return;
        }

        Console.Write("Enter amount: ");
        string? input = Console.ReadLine();

        if (!decimal.TryParse(input, out decimal amount))
        {
            Console.WriteLine("Invalid amount. Enter a number.");
            return;
        }

        if (amount <= 0)
        {
            Console.WriteLine("Amount must be greater than zero.");
            return;
        }

        paymentService.ProcessTransaction(paymentMethod, amount); 
    }



    static void ViewPaymentHistory()
    {
        Console.WriteLine("\n------------- PAYMENT HISTORY -------------");

        var history = paymentService.GetPaymentHistory();

        if (history.Count == 0)
        {
            Console.WriteLine("No transactions found.");
            return;
        }

        Console.WriteLine(
            $"{"Transaction ID",-18} {"Method",-15} {"Amount",12}  {"Date",18}");

        Console.WriteLine("--------------------------------------------");

        foreach (var transaction in history)
        {
            Console.WriteLine(
                $"{transaction.transactionId,-18} " +
                $"{transaction.paymentMethod,-15} " +
                $"{transaction.amount,9:F2} EGP " +
                $"{transaction.transactionDate,20:yyyy-MM-dd HH:mm:ss}");
        }

        Console.WriteLine("--------------------------------------------");
        Console.WriteLine(
            $"Total Transactions: " +
            $"{paymentService.GetTotalTransactions()}");
    }

}
