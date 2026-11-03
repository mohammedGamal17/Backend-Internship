using Library_Management_System.Exceptions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library_Management_System.UI
{
    public static class Helpers
    {

        public const string CancelHint = "(enter 0 to cancel)";

        //data entry helpers 
        private static string ReadLine()
        {
            string? line = Console.ReadLine();
            if (line is null)
            {
                Console.WriteLine();
                Environment.Exit(0);
            }
            return line;
        }

        public static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = ReadLine();

                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return value;

                Console.WriteLine(string.IsNullOrWhiteSpace(input)
                    ? "Input cannot be empty."
                    : $"\"{input.Trim()}\" is not valid. Please enter a number from {min} to {max}.");
            }
        }

        public static int ReadId(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} {CancelHint}: ");
                string input = ReadLine();

                if (int.TryParse(input, out int id))
                {
                    if (id == 0) throw new InvalidDataException();
                    if (id > 0) return id;
                }
                Console.WriteLine("Please enter a valid positive number for the ID.");
            }
        }

        public static string ReadRequiredText(string prompt, int maxLength = 100)
        {
            while (true)
            {
                Console.Write($"{prompt} {CancelHint}: ");
                string input = ReadLine().Trim();

                if (input == "0") throw new InvalidOperationException();

                if (input.Length == 0)
                    Console.WriteLine("This field cannot be empty.");
                else if (input.Length > maxLength)
                    Console.WriteLine($"Please keep it under {maxLength} characters.");
                else
                    return input;
            }
        }

        // for repetition
        public static bool ReadYesNo(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (y/n): ");
                string input = ReadLine().Trim().ToLower();
                if (input is "y" or "yes") return true;
                if (input is "n" or "no" or "") return false;
                Console.WriteLine("Please answer y or n.");
            }
        }
        // skipping
        public static bool? ReadOptionalYesNo(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (y/n, press Enter to keep): ");
                string input = ReadLine().Trim().ToLower();
                if (input.Length == 0) return null;
                if (input is "y" or "yes") return true;
                if (input is "n" or "no") return false;
                Console.WriteLine("Please answer y or n, or press Enter to keep it.");
            }
        }

        public static string ReadOptionalText(string prompt)
        {
            Console.Write($"{prompt} (optional, press Enter to skip): ");
            return ReadLine().Trim();
        }

        public static int ReadOptionalInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write($"{prompt} (optional, press Enter to skip): ");
                string input = ReadLine().Trim();
                if (string.IsNullOrEmpty(input))
                    return -1;
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return value;
                Console.WriteLine($"\"{input}\" is not valid. Please enter a number from {min} to {max}, or press Enter to skip.");
            }
        }

        public static List<int> ReadIdList(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} {CancelHint}: ");
                string input = ReadLine().Trim();
                if (input == "0") throw new InvalidOperationException("Operation cancelled.");

                string[] parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var ids = new List<int>();
                bool allValid = parts.Length > 0;

                foreach (string part in parts)
                {
                    if (int.TryParse(part, out int id) && id > 0) ids.Add(id);
                    else allValid = false;
                }

                if (allValid) return ids;
                Console.WriteLine("Please enter one or more positive ID numbers, separated by commas (e.g. 3, 7).");
            }
        }

        // menu helpers

        public static void MenuRunner(string title, List<(int Number, string Label, Action Handler)> options,
                          string exitLabel, bool pauseAfterEach = true)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine($"   {title}");
                Console.WriteLine("========================================");
                foreach (var option in options)
                    Console.WriteLine($"{option.Number}. {option.Label}");
                Console.WriteLine($"0. {exitLabel}");
                Console.WriteLine();

                int choice = ReadInt("Enter your choice: ", 0, options.Count);
                if (choice == 0)
                    return;                                    // leave this menu

                var selected = options.First(o => o.Number == choice);
                Console.WriteLine(selected.Label);

                try
                {
                    selected.Handler();
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Operation cancelled.");
                }
                catch (LibraryException ex)
                {
                    Console.WriteLine(ex.Message);     
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Database error: {ex.Message}");
                }

                if (pauseAfterEach) Pause();
            }
        }

        public static void Pause()
        {
            Console.WriteLine();
            Console.Write("Press Enter to return to the menu...");
            Console.ReadLine();
        }
    }
}