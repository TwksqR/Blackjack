namespace Blackjack;

public static class Dealer
{
    public static Queue<Card> Deck { get; set; } = CreateDeck(6);

    public static Hand Hand { get; } = new();

    public static List<Card> DiscardPile { get; } = new();

    private static readonly Random Rand = new(DateTime.Now.Millisecond);

    private static Queue<Card> CreateDeck(int decks)
    {
        var deck = new Queue<Card>();

        for (int i = 0; i < decks; i++)
        {
            for (int rank = 1; rank <= 13; rank++)
            {
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    deck.Enqueue(new Card(rank, suit));
                }
            }
        }

        return deck;
    }

    public static void Shuffle<T>(this IEnumerable<T> cards)
    {
        T[] cardsArray = cards.ToArray();

        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        for (int n = cardsArray.Length - 1; n > 0; n--)
        {
            int k = Rand.Next(0, n + 1);

            (cardsArray[n], cardsArray[k]) = (cardsArray[k], cardsArray[n]);
        }

        cards = cardsArray;
    }

    public static void ReshuffleDeck()
    {
        Deck = CreateDeck(8);
        Deck.Shuffle();
        Console.WriteLine("\nReshuffling deck. (A reshuffle occurs when there are 234 or less cards in the deck)");
    }

    public static void DealCard(this Hand hand, Queue<Card> deck, bool dealtCardIsToBeRevealed)
    {
        Card dealtCard = deck.Dequeue();

        dealtCard.RevealCard(dealtCardIsToBeRevealed);

        hand.Cards.Add(dealtCard);
    }

    public static void DealHand()
    {
        ConsoleColor regularDealerHandColor = ConsoleColor.Magenta;
        ConsoleColor bustedDealerHandColor = ConsoleColor.Red;

        while (Hand.Value < 17) // For this version of Blackjack, the dealer must stand on all 17s
        {
            Hand.DealCard(Deck, true);

            Console.Clear();

            ConsoleColor dealerHandColor = (Hand.IsBusted) ? bustedDealerHandColor : regularDealerHandColor;

            Console.WriteLine(Hand.DisplayCards());
            GameManager.WriteColoredLine(Hand.Value, dealerHandColor);

            if (Hand.IsBusted)
            {
                GameManager.WriteColoredLine("\nBust!", ConsoleColor.Red);
            }

            Thread.Sleep(2000);
        }
    }
}