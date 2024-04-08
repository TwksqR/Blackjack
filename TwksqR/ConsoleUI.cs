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

    public static int DisplayMenu<T>(IEnumerable<T> options, int left, int top)
    {
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

        return selectedOptionIndex;
    }

    public static bool TryReadInt(string text, out int output)
    {
        Console.CursorVisible = true;

        Console.WriteLine(text);

        string input = Console.ReadLine() ?? "";
        bool isVerified = int.TryParse(input, out output);

        return isVerified;
    }

    public static bool TryReadInt(string text, ConsoleColor textColor, out int output)
    {
        Console.CursorVisible = true;

        WriteColoredLine(text, textColor);

        string input = Console.ReadLine() ?? "";
        bool isVerified = int.TryParse(input, out output);

        return isVerified;
    }

    public static bool TryRead<T>(out object? output)
    {
        T? type = default;
        bool isVerified;

        string input = Console.ReadLine() ?? "";

        switch (type)
        {
            case string:
                isVerified = true;
                output = input;
                break;

            case int:
                isVerified = int.TryParse(input, out int outputInt);
                output = outputInt;
                break;

            case decimal:
                isVerified = decimal.TryParse(input, out decimal outputDecimal);
                output = outputDecimal;
                break;

            default:
                isVerified = false;
                output = null;
                break;
        }

        return isVerified;
    }

    public static void DisplayPressEnter()
    {
        DisplayButton("Press [Enter] to continue");
    }

    public static void DisplayButton(string button)
    {
        WriteColoredLine("\n" + button, ConsoleColor.Yellow);
        while (Console.ReadKey().Key != ConsoleKey.Enter) {}
    }

    public static void DisplayButton(string button, int offset)
    {
        Console.SetCursorPosition(0, offset);

        WriteColoredLine("\n" + button, ConsoleColor.Yellow);
        while (Console.ReadKey().Key != ConsoleKey.Enter) {}
    }
}