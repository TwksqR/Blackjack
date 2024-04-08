namespace Twksqr.Blackjack;

using TwksqR;

public static class GameManager
{
    public static List<Player> Players { get; private set; } = new();

    public static int MinBet { get; } = 1;
    public static int MaxBet { get; } = 50;

    public static bool DoubledDownCardsAreHidden { get; } = true;


    public static void Execute()
    {
        Console.Clear();
        Console.CursorVisible = false;

        ConsoleUI.WriteColoredLine("Blackjack Simulator", ConsoleColor.Magenta);
        ConsoleUI.WriteColoredLine("\nMade by TwksqR\nDiscord: twskqr\nReddit: u/GD_Stalker", ConsoleColor.DarkGray);

        ConsoleUI.DisplayButton("Let's play!", 5);

        int playerCount;
        bool playerCountIsValid;

        do
        {
            Console.Clear();

            ConsoleUI.TryReadInt("Enter player count:", ConsoleColor.Cyan, out playerCount);

            playerCountIsValid = (0 < playerCount) && (playerCount <= 7);
        }
        while (!playerCountIsValid);

        Console.CursorVisible = false;
        
        for (int playerNumber = 1; playerNumber <= playerCount; playerNumber++)
        {
            string playerName;
            bool playerNameIsVerified = false;

            Console.CursorVisible = true;

            do
            {
                Console.Clear();

                ConsoleUI.WriteColoredLine($"Enter Player {playerNumber}'s name:", ConsoleColor.Cyan);

                playerName = Console.ReadLine() ?? "";

                bool playerNameIsTaken = Players.Any(previousPlayer => previousPlayer.Name == playerName);
                
                if (playerName == "")
                {
                    playerName = "Player " + playerNumber;

                    playerNameIsVerified = true;
                }
                else if (playerNameIsTaken)
                {
                    // FIXME: #13: "This player name is already taken." will not show up due to Console.Clear() in do while loop
                    Console.WriteLine("This player name is already taken.");
                }
                else
                {
                    playerNameIsVerified = true;
                }
            }
            while (!playerNameIsVerified);

            Console.CursorVisible = false;

            Players.Add(new Player(playerName));
        }

        PlayGame();
    }

    public static void PlayGame()
    {
        int roundNumber = 0;

        // NOTE: Comment to play with unshuffled deck
        Dealer.ShuffleDeck();

        /*
        Dealer.Deck.Insert(0, new(11, Suit.Spades));
        Dealer.Deck.Insert(0, new(1, Suit.Spades));
        Dealer.Deck.Insert(0, new(6, Suit.Spades));
        Dealer.Deck.Insert(0, new(1, Suit.Spades));
        */

        do
        {
            roundNumber++;

            PlayRound(roundNumber);

            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Winnings == 0)
                {
                    ConsoleUI.WriteColoredLine($"{Players[i].Name} has bust out!", ConsoleColor.Red);

                    Players.RemoveAt(i);

                    i--;

                    Thread.Sleep(2000);
                }
            }

            for (int i = 0; i < Players.Count; i++)
            {
                Console.Clear();

                ConsoleUI.WriteColoredLine($"{Players[i].Name}, play again?", ConsoleColor.Cyan);
                ConsoleUI.WriteColoredLine($"Winnings: {string.Format("{0:C}", Players[i].Winnings)}", ConsoleColor.Green);

                var options = new Option[]
                {
                    new("Play again", PlayAgain),
                    new("Walk out", WalkOut)
                };

                static void PlayAgain(Player player) {}

                static void WalkOut(Player player)
                {
                    Console.Clear();

                    ConsoleUI.WriteColoredLine($"{player.Name} has walked out with {string.Format("{0:C}", player.Winnings)}!", ConsoleColor.Green);

                    Players.Remove(player);

                    Thread.Sleep(2000);
                }

                Option selectedOption = DisplayMenu(options, 0, 3);

                selectedOption.Action(Players[i]);

                if (selectedOption.Name == "Walk out")
                {
                    i--;
                }
            }
        }
        while (Players.Count > 0);
    }

    public static void PlayRound(int roundNumber)
    {
        Console.Clear();

        ConsoleUI.WriteColoredLine($"Round {roundNumber}", ConsoleColor.Magenta);
        
        Console.WriteLine($"\n{Dealer.Deck.Count} cards remaining in the deck");

        if (Dealer.Deck.Count <= 234)
        {
            Dealer.ReshuffleDeck();
        }

        Thread.Sleep(2000);

        Dealer.Hand.Clear();

        Dealer.Hand.DealCard(Dealer.Deck, false);
        Dealer.Hand.DealCard(Dealer.Deck, true);

        Console.CursorVisible = true;

        foreach (var player in Players)
        {
            player.Hands.Clear();

            int playerBet;
            bool playerBetIsValid;

            do
            {
                Console.Clear();

                ConsoleUI.WriteColoredLine($"{player.Name}, enter your bet. (min: {string.Format("{0:C}", MinBet)}, max: {string.Format("{0:C}", MaxBet)}, must be a whole number)", ConsoleColor.Cyan);
                ConsoleUI.TryReadInt($"Winnings: {string.Format("{0:C}", player.Winnings)}", ConsoleColor.Green, out playerBet);

                playerBetIsValid = (playerBet <= player.Winnings) && (playerBet >= MinBet) && (playerBet <= MaxBet);
            }
            while (!playerBetIsValid);
            
            Console.CursorVisible = false;

            Console.Clear();

            player.Winnings -= playerBet;

            Console.WriteLine($"{player.Name} has bet {string.Format("{0:C}", playerBet)}.");
            ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Winnings)} (-{string.Format("{0:C}", playerBet)})", ConsoleColor.Red);

            var hand = new PlayerHand(playerBet);

            hand.DealCard(Dealer.Deck, true);
            hand.DealCard(Dealer.Deck, true);

            player.Hands.Add(hand);

            Thread.Sleep(2000);
        }

        Console.CursorVisible = false;

        foreach (Player player in Players)
        {
            for (int i = 0; i < player.Hands.Count; i++)
            {
                // FIXME: #1: Index iterating over player hands is out of range after that player splits a hand
                var hand = player.Hands[i];

                do
                {
                    Console.Clear();

                    if (hand.Cards.Count == 1)
                    {
                        hand.DealCard(Dealer.Deck, true);
                    }

                    DisplayHands(hand, player);

                    List<Option> turnOptions = hand.GetTurnOptions(player);

                    Option selectedTurnOption = DisplayMenu(turnOptions, 0, 10);

                    selectedTurnOption.Action(player);

                    Console.Clear();

                    DisplayHands(hand, player);

                    if (hand.IsBusted)
                    {
                        ConsoleUI.WriteColoredLine("\nBust", ConsoleColor.Red);
                        ConsoleUI.WriteColoredLine(string.Format("{0:C}", player.Winnings), ConsoleColor.Red);

                        hand.Bet = 0;

                        hand.IsResolved = true;
                    }
                    else if (hand.IsSurrendered)
                    {
                        ConsoleUI.WriteColoredLine("\nSurrender!", ConsoleColor.Red);
                        ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Winnings)} (+{string.Format("{0:C}", hand.Bet / 2m)})", ConsoleColor.Red);
                    
                        hand.Bet = 0;

                        hand.IsResolved = true;
                    }
                    if (hand.IsSplit)
                    {
                        i--;

                        Thread.Sleep(2000);
                    }
                }
                while (!hand.IsResolved);

                Thread.Sleep(2000);
            }
        }        

        if (Players.Any(player => player.Hands.Any(hand => !hand.IsBusted && !hand.IsSurrendered)))
        {
            Console.Clear();

            Dealer.ResolveHand();

            foreach (Player player in Players)
            {
                foreach (var hand in player.Hands)
                {
                    ConsoleColor playerColor;

                    if ((DoubledDownCardsAreHidden) && (hand.IsDoubledDown))
                    {
                        playerColor = ConsoleColor.Magenta;

                        Console.Clear();

                        ConsoleUI.WriteColoredLine($"Revealing {player.Name}'s hand...\n", ConsoleColor.Cyan);

                        Console.WriteLine(hand.DisplayCards());
                        ConsoleUI.WriteColoredLine(hand.Value, playerColor);
                        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);
                        Console.WriteLine();
                        ConsoleUI.WriteColoredLine(player.Name, ConsoleColor.Cyan);

                        Thread.Sleep(2000);

                        hand.Cards[^1].IsFaceUp = true;
                        hand.UpdateValue();

                        if (hand.IsBusted)
                        {
                            playerColor = ConsoleColor.Red;
                        }

                        Console.Clear();

                        ConsoleUI.WriteColoredLine("Revealing card...\n", ConsoleColor.Cyan);

                        Console.WriteLine(hand.DisplayCards());
                        ConsoleUI.WriteColoredLine(hand.Value, playerColor);
                        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);
                        Console.WriteLine();
                        ConsoleUI.WriteColoredLine(player.Name, ConsoleColor.Cyan);

                        if (hand.IsBusted)
                        {
                            ConsoleUI.WriteColoredLine("\nBust", playerColor);
                            ConsoleUI.WriteColoredLine(string.Format("{0:C}", player.Winnings), ConsoleColor.Red);

                            hand.Bet = 0;
                        }

                        Thread.Sleep(2000);
                    }

                    if (hand.IsSurrendered || hand.IsBusted)
                    {
                        continue;
                    }

                    Console.Clear();

                    ConsoleColor winColor = ConsoleColor.Green;
                    ConsoleColor loseColor = ConsoleColor.Red;
                    ConsoleColor tieColor = ConsoleColor.DarkGray;

                    ConsoleColor dealerColor = tieColor;
                    playerColor = tieColor;
                    
                    string playerHandResult = "Push";

                    if ((hand.Value > Dealer.Hand.Value) || (Dealer.Hand.IsBusted))
                    {
                        dealerColor = loseColor;
                        playerColor = winColor;

                        playerHandResult = "Win";

                        hand.Bet *= 2;

                        player.Winnings += hand.Bet;
                    }
                    else if (hand.Value < Dealer.Hand.Value)
                    {
                        dealerColor = winColor;
                        playerColor = loseColor;

                        playerHandResult = "Lose";

                        hand.Bet = 0;
                    }
                    else
                    {
                        player.Winnings += hand.Bet;
                    }

                    Console.WriteLine($"{Dealer.Hand.IsBusted}\n");

                    Console.WriteLine(Dealer.Hand.DisplayCards());
                    ConsoleUI.WriteColoredLine(Dealer.Hand.Value, dealerColor);
                    Console.WriteLine();
                    Console.WriteLine(hand.DisplayCards());
                    ConsoleUI.WriteColoredLine(hand.Value, playerColor);
                    ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);
                    Console.WriteLine();
                    ConsoleUI.WriteColoredLine(player.Name, ConsoleColor.Cyan);
                    ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Winnings)} (+{string.Format("{0:C}", hand.Bet)})", playerColor);
                    Console.WriteLine();
                    ConsoleUI.WriteColoredLine(playerHandResult, playerColor);

                    ConsoleUI.DisplayPressEnter();
                }
            }
        }

        Console.Clear();
    }

    public static void DisplayHands(PlayerHand hand, Player owner)
    {
        ConsoleColor regularPlayerHandColor = ConsoleColor.Magenta;
        ConsoleColor bustedPlayerHandColor = ConsoleColor.Red;

        ConsoleColor playerColor = (hand.IsBusted) ? bustedPlayerHandColor : regularPlayerHandColor;

        Console.WriteLine(Dealer.Hand.DisplayCards());
        ConsoleUI.WriteColoredLine(Dealer.Hand.Value, ConsoleColor.Magenta);

        Console.WriteLine();

        Console.WriteLine(hand.DisplayCards());
        ConsoleUI.WriteColoredLine(hand.Value, playerColor);
        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

        Console.WriteLine();

        ConsoleUI.WriteColoredLine(owner.Name, ConsoleColor.Cyan);
        ConsoleUI.WriteColoredLine(string.Format("{0:C}", owner.Winnings), ConsoleColor.DarkGray);
    }

    public static Option DisplayMenu(this IEnumerable<Option> options, int left, int top)
    {
        var optionNames = options.Select(option => option.Name);
        
        int selectedOptionIndex = ConsoleUI.DisplayMenu(optionNames, left, top);

        return options.ElementAt(selectedOptionIndex);
    }
}