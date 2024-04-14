namespace Twksqr.Blackjack;

public sealed class Player
{
    public string Name { get; }

    public decimal Winnings { get; set; } = Settings.initialWinnings;

    public List<PlayerHand> Hands { get; } = new();

    public Player(string name)
    {
        Name = name;
    }
}