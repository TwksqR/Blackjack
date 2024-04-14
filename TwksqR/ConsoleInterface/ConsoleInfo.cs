namespace Twksqr.ConsoleInterface;

internal static class ConsoleInfo
{
    private static int savedCursorLeft = 0;
    private static int savedCursorTop = 0;

    private static ConsoleColor savedForegroundColor = ConsoleColor.White;
    private static ConsoleColor savedBackgroundColor = ConsoleColor.Black;

    internal static readonly ConsoleColor unselectedOptionColor  = ConsoleColor.Blue;
    internal static readonly ConsoleColor SelectedOptionColor = ConsoleColor.Yellow;

    internal static void SaveCursorPosition()
    {
        (savedCursorLeft, savedCursorTop) = Console.GetCursorPosition();
    }

    internal static void LoadCursorPosition()
    {
        Console.SetCursorPosition(savedCursorLeft, savedCursorTop);
    }

    internal static void SaveForegroundColor()
    {
        savedForegroundColor = Console.ForegroundColor;
    }

    internal static void LoadForegroundColor()
    {
        Console.ForegroundColor = savedForegroundColor;
    }

    internal static void SaveBackgroundColor()
    {
        savedBackgroundColor = Console.BackgroundColor;
    }

    internal static void LoadBackgroundColor()
    {
        Console.BackgroundColor = savedBackgroundColor;
    }
}