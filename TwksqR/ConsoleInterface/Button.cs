namespace Twksqr.ConsoleInterface;

public class Button
{
    public static void DisplayButtonPressEnter()
    {
        DisplayButton("Press [Enter] to continue");
    }

    public static void DisplayButton(string button)
    {
        Console.CursorVisible = false;

        ConsoleInterfaceUtils.WriteColored($"\n{button}", ConsoleInfo.SelectedOptionColor);
        
        while (Console.ReadKey().Key != ConsoleKey.Enter) {}

        Console.CursorVisible = true;
    }

    public static void DisplayButton(string button, int left, int top)
    {
        ConsoleInfo.SaveCursorPosition();
        Console.SetCursorPosition(left, top);
        Console.CursorVisible = false;

        WriteColoredLine(button, ConsoleInfo.SelectedOptionColor);
        
        while (Console.ReadKey().Key != ConsoleKey.Enter) {}

        ConsoleInfo.LoadCursorPosition();
        Console.CursorVisible = true;
    }
}