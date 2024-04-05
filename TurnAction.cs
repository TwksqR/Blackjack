namespace Blackjack;

public sealed class TurnAction
{
    public Action<Player> Selection;

    private string _name = "Turn Action";
    public string Name
    {
        get { return _name; }

        set
        {
            _name = (!string.IsNullOrWhiteSpace(value)) ? value : _name;
        }
    }

    public TurnAction(string name, Action<Player> selection)
    {
        Name = name;
        Selection = selection;
    }
}