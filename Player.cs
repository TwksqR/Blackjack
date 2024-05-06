namespace Twksqr.Blackjack;

public sealed class Player
{
    public string Name { get; }

    public decimal Money { get; set; } = Settings.InitialMoney;

    public List<PlayerHand> Hands { get; } = new();

    public Player(string name)
    {
        Name = name;
    }
}