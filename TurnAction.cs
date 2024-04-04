namespace Blackjack;

public sealed class TurnAction
{
    public delegate void OptionEventHandler(Player owner);

    private string _name = "Turn Action";
    public string Name
    {
        get { return _name; }

        set
        {
            _name = (!string.IsNullOrWhiteSpace(value)) ? value : _name;
        }
    }

    public OptionEventHandler Selection { get; set; }

    public TurnAction(string name, OptionEventHandler selection)
    {
        Name = name;
        Selection = selection;
    }
}