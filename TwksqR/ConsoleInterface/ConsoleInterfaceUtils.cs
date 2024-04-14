namespace Twksqr.ConsoleInterface;

public static class ConsoleInterfaceUtils
{
    public static void WriteColored(object? text, ConsoleColor color)
    {
        ConsoleInfo.SaveForegroundColor();
        Console.ForegroundColor = color;

        Console.Write(text);

        ConsoleInfo.LoadForegroundColor();
    }

    public static void WriteColoredLine(object? text, ConsoleColor color)
    {
        ConsoleInfo.SaveForegroundColor();
        Console.ForegroundColor = color;

        Console.WriteLine(text);

        ConsoleInfo.LoadForegroundColor();
    }

    public static bool TryRead(out int output)
    {
        Console.CursorVisible = true;

        string input = Console.ReadLine() ?? "";

        return int.TryParse(input, out output);
    }

    public static bool TryRead(out decimal output)
    {
        Console.CursorVisible = true;

        string input = Console.ReadLine() ?? "";

        return decimal.TryParse(input, out output);
    }
}