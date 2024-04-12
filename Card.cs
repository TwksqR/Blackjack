namespace Twksqr.Blackjack;

public class Card
{
    public int Rank { get; }
    public Suit Suit { get; }
    
    private int _value;
    public int Value
    {
        get { return _value; }
         
        set
        {
            _value = Math.Clamp(value, 1 , 11);
        }
    }

    private string _shortName = "??";
    public string ShortName
    {
        get
        { 
            return (IsFaceUp) ? _shortName : "??";
        }

        private set { _shortName = value; }
    }

    public bool IsFaceUp { get; set; } = false;

    public Card(int rank, Suit suit)
    {
        Rank = Math.Clamp(rank, 1, 13);
        Suit = suit;

        // All face cards (Jacks, Queens, Kings) are worth 10
        Value = Math.Clamp(Rank, 1, 10);

        ShortName = GetShortName();

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