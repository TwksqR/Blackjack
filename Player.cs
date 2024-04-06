namespace Blackjack;

public sealed class Player
{
    public string Name { get; }

    public decimal Bankroll { get; set; } = 50m;

    public List<PlayerHand> Hands { get; } = new();

    public Player(string name)
    {
        Name = name;
    }
}