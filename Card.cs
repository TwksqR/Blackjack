namespace Twksqr.Blackjack;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Card : INotifyPropertyChanged
{
    public int Rank { get; }
    public Suit Suit { get; }
    
    public int Value { get; }

    private readonly string _shortName;
    public string ShortName
    {
        get
        { 
            return (IsFaceUp) ? _shortName : "??";
        }
    }

    private bool _isFaceUp = false;
    public bool IsFaceUp
    {
        get { return _isFaceUp; }

        set
        {
            _isFaceUp = value;

            NotifyPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Card(int rank, Suit suit)
    {
        Rank = Math.Clamp(rank, 1, 13);
        Suit = suit;

        // All face cards (Jacks, Queens, Kings) are worth 10
        Value = Math.Clamp(Rank, 1, 10);

        _shortName = GetShortName();

        string GetShortName()
        {
            string parsedRank = Rank switch
            {
                1  => "A", // Ace
                11 => "J", // Jack
                12 => "Q", // Queen
                13 => "K", // King
                _  => Rank.ToString()
            };

            char parsedSuit = Suit switch
            {
                Suit.Spades   => '♠',
                Suit.Diamonds => '♦',
                Suit.Clubs    => '♣',
                Suit.Hearts   => '♥',
                _  => '_' // If this is the result, then something has gone VERY wrong
            };

            return parsedRank + parsedSuit;
        }
    }

    public static bool Equals(Card card1, Card card2)
    {
        return (card1.Rank == card2.Rank) && (card1.Suit == card2.Suit);
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum Suit
{
    Spades,
    Diamonds,
    Clubs,
    Hearts
}