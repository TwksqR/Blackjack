namespace Twksqr.Blackjack;

public class Option
{
    public Action<Player> Action;

    private string _name = "Option";
    public string Name
    {
        get => _name;
        
        set => _name = (!string.IsNullOrWhiteSpace(value)) ? value : _name;
    }

    public Option(string name, Action<Player> action)
    {
        Name = name;
        Action = action;
    }
}