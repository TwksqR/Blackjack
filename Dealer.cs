namespace Twksqr.Blackjack;

using TwksqR;

public static class Dealer
{
    public static List<Card> Deck { get; private set; } = CreateDeck(8).ToList();

    public static Hand Hand { get; } = new();

    private static readonly Random _rand = new(DateTime.Now.Millisecond);

    private static IEnumerable<Card> CreateDeck(int decks)
    {
        for (int i = 0; i < decks; i++)
        {
            for (int rank = 1; rank <= 13; rank++)
            {
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    yield return new Card(rank, suit);
                }
            }
        }
    }

    public static void ShuffleDeck(this IList<Card> deck)
    {
        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        for (int n = deck.Count - 1; n > 0; n--)
        {
            int k = _rand.Next(0, n + 1);

            (deck[n], deck[k]) = (deck[k], deck[n]);
        }
    }

    public static void ReshuffleDeck()
    {
        Deck = CreateDeck(8).ToList();
        ShuffleDeck(Deck);
        Console.WriteLine("\nReshuffling deck. (A reshuffle occurs when there are 234 or less cards in the deck)");
    }

    public static void AddDebugCards(this IList<Card> collection)
    {
        var cardValues = new int[]
        {
            
        };

        foreach (int cardValue in cardValues)
        {
            collection.Add(new(cardValue, Suit.Spades));
        }
    }

    public static void ResolveHand()
    {
        ConsoleUI.WriteColoredLine("Revealing dealer's hand...", ConsoleColor.Cyan);

        Console.WriteLine($"\n{Hand.GetCardShortNames()}");

        Thread.Sleep(2000);

        Hand[0].IsFaceUp = true;

        Console.Clear();

        ConsoleUI.WriteColoredLine("Resolving dealer's hand...", ConsoleColor.Cyan);

        Console.WriteLine($"\n{Hand.GetCardShortNames()}");
        ConsoleUI.WriteColoredLine(Hand.Value, ConsoleColor.Magenta);

        while (Hand.Value < 17)
        {
            Thread.Sleep(2000);

            Hand.DealCard(Deck, true);

            Console.Clear();

            ConsoleColor dealerHandColor = (Hand.Status == HandStatus.Busted) ? ConsoleColor.Red : ConsoleColor.Magenta;

            ConsoleUI.WriteColoredLine("Resolving dealer's hand...", ConsoleColor.Cyan);
            Console.WriteLine($"\n{Hand.GetCardShortNames()}");
            ConsoleUI.WriteColoredLine(Hand.Value, dealerHandColor);
        }

        switch (Hand.Status)
        {
            case HandStatus.Stood:
                ConsoleUI.WriteColoredLine("\nStand", ConsoleColor.Blue);;
                break;

            case HandStatus.Busted:
                ConsoleUI.WriteColoredLine("\nBust", ConsoleColor.Red);
                break;
        }

        Thread.Sleep(2000);
    }
}