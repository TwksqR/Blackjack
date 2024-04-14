namespace Twksqr.ConsoleInterface;

public static class Menu
{
    public static int DisplayMenu<T>(IEnumerable<T> options)
    {
        Console.CursorVisible = false;
        ConsoleInfo.SaveCursorPosition();

        int left = Console.CursorLeft;
        int top = Console.CursorTop + 1;

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
        ConsoleInfo.LoadCursorPosition();

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
}