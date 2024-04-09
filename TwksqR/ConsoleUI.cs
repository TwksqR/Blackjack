namespace TwksqR;

public static class ConsoleUI
{
    private static readonly ConsoleColor _unselectedOptionColor = ConsoleColor.Blue;
    private static readonly ConsoleColor _selectedOptionColor = ConsoleColor.Yellow;

    public static void WriteColored(object? text, ConsoleColor color)
    {
        var currentForegroundColor = Console.ForegroundColor;
        Console.ForegroundColor = color;

        Console.Write(text);

        Console.ForegroundColor = currentForegroundColor;
    }

    public static void WriteColoredLine(object? text, ConsoleColor color)
    {
        var currentForegroundColor = Console.ForegroundColor;
        Console.ForegroundColor = color;

        Console.WriteLine(text);

        Console.ForegroundColor = currentForegroundColor;
    }

    public static int DisplayMenu<T>(IEnumerable<T> options)
    {
        Console.CursorVisible = false;

        int left = Console.CursorLeft;
        int top = Console.CursorTop;

        int selectedOptionIndex = 0;

        ConsoleKeyInfo keyInfo;

        do
        {
            Console.SetCursorPosition(left, top);

            for (int i = 0; i < options.Count(); i++)
            {
                if (options.ElementAt(selectedOptionIndex) == null || options.ElementAt(selectedOptionIndex)?.ToString() == "")
                {
                    continue;
                }

                var optionColor = (i == selectedOptionIndex) ? _selectedOptionColor : _unselectedOptionColor;

                WriteColoredLine(options.ElementAt(i), optionColor);
            }

            keyInfo = Console.ReadKey(false);

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow: 
                    selectedOptionIndex--;
                    break;

                case ConsoleKey.DownArrow: 
                    selectedOptionIndex++;
                    break;
            }

            selectedOptionIndex = Math.Clamp(selectedOptionIndex, 0, options.Count() - 1);
        }
        while (keyInfo.Key != ConsoleKey.Enter);

        Console.CursorVisible = true;

        return selectedOptionIndex;
    }

    public static int DisplayMenu<T>(IEnumerable<T> options, int left, int top)
    {
        Console.CursorVisible = false;

        int selectedOptionIndex = 0;

        ConsoleKeyInfo keyInfo;

        do
        {
            Console.SetCursorPosition(left, top);

            for (int i = 0; i < options.Count(); i++)
            {
                if (options.ElementAt(selectedOptionIndex) == null || options.ElementAt(selectedOptionIndex)?.ToString() == "")
                {
                    continue;
                }

                var optionColor = (i == selectedOptionIndex) ? _selectedOptionColor : _unselectedOptionColor;

                WriteColoredLine(options.ElementAt(i), optionColor);
            }

            keyInfo = Console.ReadKey(false);

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow: 
                    selectedOptionIndex--;
                    break;

                case ConsoleKey.DownArrow: 
                    selectedOptionIndex++;
                    break;
            }

            selectedOptionIndex = Math.Clamp(selectedOptionIndex, 0, options.Count() - 1);
        }
        while (keyInfo.Key != ConsoleKey.Enter);

        Console.CursorVisible = true;

        return selectedOptionIndex;
    }

    public static bool TryRead(out int output)
    {
        Console.CursorVisible = true;

        string input = Console.ReadLine() ?? "";

        bool isVerified = int.TryParse(input, out output);

        return isVerified;
    }

    public static bool TryRead(out decimal output)
    {
        Console.CursorVisible = true;

        string input = Console.ReadLine() ?? "";

        bool isVerified = decimal.TryParse(input, out output);

        return isVerified;
    }

    public static void DisplayButtonPressEnter()
    {
        DisplayButton("Press [Enter] to continue");
    }

    public static void DisplayButton(string button)
    {
        Console.CursorVisible = false;

        WriteColoredLine($"\n{button}", _selectedOptionColor);
        
        while (Console.ReadKey().Key != ConsoleKey.Enter) {}

        Console.CursorVisible = true;
    }

    public static void DisplayButton(string button, int left, int top)
    {
        Console.SetCursorPosition(left, top);

        Console.CursorVisible = false;

        WriteColoredLine(button, _selectedOptionColor);
        
        while (Console.ReadKey().Key != ConsoleKey.Enter) {}

        Console.CursorVisible = true;
    }
}