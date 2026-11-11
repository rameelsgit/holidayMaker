namespace HolidayMaker;
public static class InputHelper
{
    // checks for empty string 
    public static string ReadString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()!;
            if (!string.IsNullOrWhiteSpace(input))
            {
                return input;
            }
            Console.WriteLine("Input cannot be empty. Try again.");
        }
    }

    // validates guest amount for searching rooms and creating a booking 
    public static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine()!, out int result) && result > 0)
            {
                return result;
            }
            Console.WriteLine("Please enter a positive number.");
        }
    }

    // validates date format for searching rooms and creating a booking 
    public static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (DateTime.TryParse(Console.ReadLine()!, out DateTime date))
            {
                return date;
            }
            Console.WriteLine("Invalid date format (YYYY-MM-DD). Try again.");
        }
    }
    // makes sure checkout date is after check in date.
    public static DateTime ReadCheckOutDate(string prompt, DateTime checkIn)
    {
        while (true)
        {
            DateTime checkOut = ReadDate(prompt);
            if (checkOut > checkIn)
            {
                return checkOut;
            }
            Console.WriteLine("Check-out must be after check-in date");
        }
    }
}