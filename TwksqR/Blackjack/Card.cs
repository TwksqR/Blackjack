namespace Twksqr.Blackjack;

public class Card
{
    public int Rank { get; }
    public Suit Suit { get; }
    
    public int Value { get; private set; }

    private string _shortName = "?";
    public string ShortName
    {
        get
        {
            return (IsFaceUp) ? _shortName : "?";
        }

        private set
        {
            _shortName = value;
        }
    }

    public bool IsFaceUp { get; private set; } = false;

    public Card(int rank, Suit suit)
    {
        Rank = rank;
        Suit = suit;

        Value = GetBlackjackValue();

        ShortName = GetShortName();

        int GetBlackjackValue()
        {
            // All face cards (Jacks, Queens, Kings) are worth 10
            // Aces have an initial value of 11 in Blackjack; for deck creation convenience, their value was set to 1
            return (Rank == 1) ? 11 : Math.Clamp(Rank, 2, 10);
        }

        string GetShortName()
        {
            string displayRank = Rank switch
            {
                1  => "A", // Ace
                11 => "J", // Jack
                12 => "Q", // Queen
                13 => "K", // King
                _  => Rank.ToString()
            };

            char displaySuit = Suit switch
            {
                Suit.Spades   => '♠',
                Suit.Diamonds => '♦',
                Suit.Clubs    => '♣',
                Suit.Hearts   => '♥',
                _  => '_' // If this is the result, then something has gone VERY wrong
            };

            return displayRank + displaySuit;
        }
    }

    public void SetValue(int newVal)
    {
        Value = Math.Clamp(newVal, 1 , 11);
    }

    public void RevealCard(bool toBeFaceUp)
    {
        IsFaceUp = toBeFaceUp;
    }

    public static bool Equals(Card card1, Card card2)
    {
        return (card1.Rank == card2.Rank) && (card1.Suit == card2.Suit);
    }
}

public enum Suit
{
    Spades,
    Diamonds,
    Clubs,
    Hearts
}