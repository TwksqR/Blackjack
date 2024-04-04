namespace Blackjack;

public static class Dealer
{
    public static Queue<Card> Deck { get; private set; } = CreateDeck(6);

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

    public static void ShuffleDeck()
    {
        var deckList = Deck.ToArray();

        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        for (int n = deckList.Length - 1; n > 0; n--)
        {
            int k = Rand.Next(0, n + 1);

            (deckList[n], deckList[k]) = (deckList[k], deckList[n]);
        }

        Deck = new(deckList);
    }

    // TODO: Replace with basic CreateDeck() and Shuffle()
    public static void ReshuffleDeck()
    {
        Deck = CreateDeck(8);
        ShuffleDeck();
        Console.WriteLine("Reshuffling deck. (A reshuffle occurs when there are 234 or less cards in the deck)");
        
        GameManager.DisplayPressEnter();
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

    // TODO: Move Dealer.DealHand() and Player.DealHand() here, and combine them into one
    /*
    public static void Play(Player player)
    {
        deck.DealCard(Deck);

        string selectedHandOption = "Hit";

        do
        {
            deck.DealCard(Deck);

            Console.Clear();

            var dealerFaceUpCards = Hand.Cards.Where(card => Dealer.Hand.Cards.IndexOf(card) > 0);
            var dealerFaceUpCardsNames = dealerFaceUpCards.Select(card => card.ShortName);
            ConsoleInterface.WriteColoredLine($"?? {string.Join(' ', dealerFaceUpCardsNames)}\n", ConsoleColor.Green);

            Console.WriteLine(string.Join(' ', Cards.Select(card => card.ShortName)));
            Console.WriteLine($"Value: {Value}");

            if (Value == 21)
            {
                if (Cards.Count == 2)
                {
                    Console.WriteLine("\nBlackjack!");
                }
                break;
            }

            if (Value > 21)
            {
                Console.WriteLine("\nBust!");
                // Winnings -= Bet;

                Reset();

                ConsoleInterface.DisplayPressEnter();

                return;
            }
            
            if (IsDoubledDown)
            {
                break;
            }

            var handOptions = GetHandOptions();

            selectedHandOption = ConsoleInterface.DisplayOptionsString(handOptions, 5);

            if (selectedHandOption == "Double Down")
            {
                IsDoubledDown = true;
            }
        }
        while (selectedHandOption != "Stand" && Cards.Count < 21);

        Console.Clear();

        Console.WriteLine(string.Join(' ', Cards.Select(card => card.ShortName)));
        Console.WriteLine($"Final Value: {Value}");

        ConsoleInterface.DisplayPressEnter();

        List<string> GetHandOptions()
        {
            var handOptions = new List<string>()
            {
                "Hit",
                "Stand"
            };

            bool handHasTwoCards = Cards.Count == 2;
            bool handIsUnsplit = player.Hands.Count == 1;

            if (handHasTwoCards && handIsUnsplit)
            {
                handOptions.Add("Double Down");
            }

            return handOptions;
        }
    }
    */
}