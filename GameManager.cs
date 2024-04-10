namespace Twksqr.Blackjack;

using TwksqR;

public static class GameManager
{
    private static readonly int _maxRounds = 0;
    private static readonly bool _playersCanLeaveBeforeMaxRoundReached = false;

    public static List<Player> Players { get; } = new();

    private static readonly int _minPlayers = 1;
    private static readonly int _maxPlayers = 7;

    public static decimal InitialWinnings { get; } = 100m;

    private static readonly int _minBet = 5;
    private static readonly int _maxBet = 50;

    public static bool DoubledDownCardsAreHidden { get; } = true;

    public static void Execute()
    {
        Console.Clear();
        Console.CursorVisible = false;

        ConsoleUI.WriteColoredLine("Blackjack Simulator", ConsoleColor.Magenta);
        ConsoleUI.WriteColoredLine("\nMade by TwksqR\nDiscord: twskqr\nReddit: u/GD_Stalker", ConsoleColor.DarkGray);

        ConsoleUI.DisplayButton("Let's play!");

        StartGame();
    }

    public static void StartGame()
    {
        int playerCount = GetPlayerCount();

        Console.CursorVisible = false;

        GetPlayerNames(playerCount);

        Console.CursorVisible = false;

        static int GetPlayerCount()
        {
            int playerCount;
            bool playerCountIsValid;

            do
            {
                Console.Clear();

                ConsoleUI.WriteColoredLine("Enter player count:", ConsoleColor.Cyan);
                ConsoleUI.TryRead(out playerCount);

                playerCountIsValid = (_minPlayers <= playerCount) && (playerCount <= _maxPlayers);
            }
            while (!playerCountIsValid);

            return playerCount;
        }

        static void GetPlayerNames(int playerCount)
        {
            for (int playerNumber = 1; playerNumber <= playerCount; playerNumber++)
            {
                string playerName;
                bool playerNameIsVerified = false;

                Console.CursorVisible = true;

                Console.Clear();

                do
                {
                    ConsoleUI.WriteColoredLine($"Enter Player {playerNumber}'s name:\n(Enter blank query to use default name)", ConsoleColor.Cyan);

                    playerName = Console.ReadLine() ?? "";

                    Console.Clear();

                    playerNameIsVerified = !Players.Any(previousPlayer => previousPlayer.Name == playerName);

                    if (!playerNameIsVerified)
                    {
                        ConsoleUI.WriteColoredLine($"\"{playerName}\" is already taken.\n", ConsoleColor.Red);
                        continue;
                    }

                    if (playerName == "")
                    {
                        playerName = $"Player {playerNumber}";
                    }
                }
                while (!playerNameIsVerified);

                Console.CursorVisible = false;

                Players.Add(new Player(playerName));
            }
        }

        int roundNumber = 0;

        // NOTE: Comment to play with unshuffled deck
        Dealer.ShuffleDeck(Dealer.Deck);

        /*
        Dealer.Deck.Insert(0, new(1, Suit.Spades));
        Dealer.Deck.Insert(0, new(13, Suit.Spades));
        Dealer.Deck.Insert(0, new(1, Suit.Spades));
        Dealer.Deck.Insert(0, new(1, Suit.Spades));
        */

        do
        {
            roundNumber++;

            PlayRound(roundNumber);

            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Winnings < _minBet)
                {
                    ConsoleUI.WriteColoredLine($"{Players[i].Name} has bust out!", ConsoleColor.Red);

                    Players.RemoveAt(i);

                    i--;

                    Thread.Sleep(2000);
                }
            }

            if ((_maxRounds > 0) && (roundNumber >= _maxRounds))
            {
                foreach (var player in Players)
                {
                    Console.Clear();

                    ConsoleUI.WriteColoredLine($"{player.Name} has finished with {string.Format("{0:C}", player.Winnings)}!", ConsoleColor.Green);

                    ConsoleUI.DisplayButtonPressEnter();
                }

                break;
            }

            if ((_maxRounds == 0) || _playersCanLeaveBeforeMaxRoundReached)
            {
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

                    var selectedOption = DisplayMenu(options, 0, 3);

                    selectedOption.Action(Players[i]);

                    if (selectedOption.Name == "Walk out")
                    {
                        i--;
                    }
                }
            }
        }
        while (Players.Count > 0);

        Console.Clear();

        ConsoleUI.WriteColoredLine("Thanks for playing!", ConsoleColor.Magenta);

        Thread.Sleep(2000);
    }

    public static void PlayRound(int roundNumber)
    {
        Console.Clear();

        string roundDisplay = (_maxRounds > 0) ? $"Round {roundNumber}/{_maxRounds}" : $"Round {roundNumber}";

        ConsoleUI.WriteColoredLine(roundDisplay, ConsoleColor.Magenta);

        Console.WriteLine($"\n{Dealer.Deck.Count} cards remaining in the deck");

        if (Dealer.Deck.Count <= 234)
        {
            Dealer.ReshuffleDeck();
        }

        Thread.Sleep(2000);

        Dealer.Hand.Reset();

        Dealer.Hand.DealCard(Dealer.Deck, false);
        Dealer.Hand.DealCard(Dealer.Deck, true);

        GetPlayerBets();

        Console.CursorVisible = false;

        if (Dealer.Hand.Cards[1].Value >= 10)
        {
            if (Dealer.Hand.Cards[1].Value == 11)
            {
                foreach (var player in Players)
                {
                    Console.Clear();

                    DisplayHands(player.Hands[0], player);

                    var insuranceOptions = player.Hands[0].GetInsuranceOptions(player);

                    ConsoleUI.WriteColoredLine("\nInsurance?", ConsoleColor.Cyan);
                    Option selectedInsuranceOption = DisplayMenu(insuranceOptions, 0, 11);

                    selectedInsuranceOption.Action(player);
                }
            }

            Console.Clear();

            ConsoleUI.WriteColoredLine("Checking dealer's hand for Blackjack...", ConsoleColor.Cyan);

            Console.WriteLine($"\n{Dealer.Hand.DisplayCards()}");
            ConsoleUI.WriteColoredLine(Dealer.Hand.Value, ConsoleColor.Magenta);

            Thread.Sleep(2000);

            Console.Clear();

            Dealer.Hand.Cards[0].IsFaceUp = true;
            Dealer.Hand.UpdateValue();

            if (Dealer.Hand.State == HandState.Blackjack)
            {
                ConsoleUI.WriteColoredLine("Blackjack!", ConsoleColor.Red);

                Console.WriteLine($"\n{Dealer.Hand.DisplayCards()}");
                ConsoleUI.WriteColoredLine(Dealer.Hand.Value, ConsoleColor.Magenta);

                Thread.Sleep(2000);

                foreach (var player in Players)
                {
                    ConsoleColor playerColor;

                    playerColor = ConsoleColor.Magenta;

                    Console.Clear();

                    ConsoleColor winColor = ConsoleColor.Green;
                    ConsoleColor loseColor = ConsoleColor.Red;
                    ConsoleColor tieColor = ConsoleColor.DarkGray;

                    ConsoleColor dealerColor = tieColor;
                    playerColor = tieColor;

                    var hand = player.Hands[0];

                    string playerHandResult = "Push";
                    string playerHandInsurancePayoutText = $"+{string.Format("{0:C}", hand.Bet)}";

                    if (hand.State == HandState.Blackjack)
                    {
                        dealerColor = tieColor;
                        playerColor = tieColor;

                        player.Winnings += hand.Bet;
                    }
                    else
                    {
                        dealerColor = winColor;
                        playerColor = loseColor;

                        playerHandResult = "Lose";

                        hand.Bet = 0;
                    }

                    if (hand.InsuranceBet > 0m)
                    {
                        hand.InsuranceBet *= 3;

                        player.Winnings += hand.InsuranceBet;

                        playerHandInsurancePayoutText = $"+{string.Format("{0:C}", hand.InsuranceBet)} insurance";
                    }
                    else
                    {
                        hand.InsuranceBet = 0;
                    }

                    Console.WriteLine($"\n{Dealer.Hand.DisplayCards()}");
                    ConsoleUI.WriteColoredLine(Dealer.Hand.Value, dealerColor);

                    Console.WriteLine($"\n{hand.DisplayCards()}");
                    ConsoleUI.WriteColoredLine(hand.Value, playerColor);
                    ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

                    ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
                    ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Winnings)} ({playerHandInsurancePayoutText})", playerColor);

                    ConsoleUI.WriteColoredLine($"\n{playerHandResult}", playerColor);

                    ConsoleUI.DisplayButtonPressEnter();
                }

                return;
            }
            else
            {
                Dealer.Hand.Cards[0].IsFaceUp = false;

                ConsoleUI.WriteColoredLine("Dealer does not have Blackjack.", ConsoleColor.White);

                Console.WriteLine($"\n{Dealer.Hand.DisplayCards()}");
                ConsoleUI.WriteColoredLine(Dealer.Hand.Value, ConsoleColor.Magenta);

                Thread.Sleep(2000);

                foreach (var player in Players)
                {
                    var hand = player.Hands[0];

                    Console.Clear();

                    if (hand.InsuranceBet > 0)
                    {
                        Console.WriteLine($"\n{Dealer.Hand.DisplayCards()}");
                        ConsoleUI.WriteColoredLine(Dealer.Hand.Value, ConsoleColor.Magenta);

                        Console.WriteLine($"\n{hand.DisplayCards()}");
                        ConsoleUI.WriteColoredLine(hand.Value, ConsoleColor.Magenta);
                        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

                        ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
                        ConsoleUI.WriteColoredLine($"(-{string.Format("{0:C}", hand.InsuranceBet)} insurance)", ConsoleColor.Red);

                        ConsoleUI.DisplayButtonPressEnter();
                    }
                }
            }
        }

        foreach (var player in Players)
        {
            var hand = player.Hands[0];

            if (hand.State != HandState.Blackjack)
            {
                continue;
            }

            hand.Bet *= 2.5m;
            player.Winnings += hand.Bet;

            Console.Clear();

            Console.WriteLine($"\n{Dealer.Hand.DisplayCards()}");
            ConsoleUI.WriteColoredLine(Dealer.Hand.Value, ConsoleColor.Magenta);

            Console.WriteLine($"\n{hand.DisplayCards()}");
            ConsoleUI.WriteColoredLine(hand.Value, ConsoleColor.Magenta);
            ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

            ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
            ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Winnings)} (+{string.Format("{0:C}", hand.Bet)})", ConsoleColor.Green);

            ConsoleUI.WriteColoredLine("\nBlackjack!", ConsoleColor.Green);

            ConsoleUI.DisplayButtonPressEnter();
        }

        ResolvePlayerTurns();

        if (Players.Any(player => player.Hands.Any(hand => (hand.State == HandState.Stood) || (hand.State == HandState.DoubledDown))))
        {
            Console.Clear();

            Dealer.ResolveHand();

            foreach (var player in Players)
            {
                foreach (var hand in player.Hands)
                {
                    ConsoleColor playerColor;

                    if (DoubledDownCardsAreHidden && (hand.State == HandState.DoubledDown))
                    {
                        playerColor = ConsoleColor.Magenta;

                        Console.Clear();

                        ConsoleUI.WriteColoredLine($"Revealing {player.Name}'s hand...", ConsoleColor.Cyan);

                        Console.WriteLine($"\n{hand.DisplayCards()}");
                        ConsoleUI.WriteColoredLine(hand.Value, playerColor);
                        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

                        ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);

                        Thread.Sleep(2000);

                        hand.Cards[^1].IsFaceUp = true;
                        hand.UpdateValue();

                        if (hand.State == HandState.Busted)
                        {
                            playerColor = ConsoleColor.Red;
                        }

                        Console.Clear();

                        ConsoleUI.WriteColoredLine($"Revealing {player.Name}'s hand...", ConsoleColor.Cyan);

                        Console.WriteLine($"\n{hand.DisplayCards()}");
                        ConsoleUI.WriteColoredLine(hand.Value, playerColor);
                        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

                        ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);

                        if (hand.State == HandState.Busted)
                        {
                            ConsoleUI.WriteColoredLine("\nBust", playerColor);
                            ConsoleUI.WriteColoredLine(string.Format("{0:C}", player.Winnings), ConsoleColor.Red);

                            hand.Bet = 0;
                        }

                        Thread.Sleep(2000);
                    }

                    if ((hand.State == HandState.Surrendered) || (hand.State == HandState.Busted) || (hand.State == HandState.Blackjack))
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

                    if ((hand.Value > Dealer.Hand.Value) || (Dealer.Hand.State == HandState.Busted))
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

                    Console.WriteLine($"\n{Dealer.Hand.DisplayCards()}");
                    ConsoleUI.WriteColoredLine(Dealer.Hand.Value, dealerColor);

                    Console.WriteLine($"\n{hand.DisplayCards()}");
                    ConsoleUI.WriteColoredLine(hand.Value, playerColor);
                    ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

                    ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
                    ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Winnings)} (+{string.Format("{0:C}", hand.Bet)})", playerColor);

                    ConsoleUI.WriteColoredLine($"\n{playerHandResult}", playerColor);

                    ConsoleUI.DisplayButtonPressEnter();
                }
            }
        }

        Console.Clear();

        static void GetPlayerBets()
        {
            foreach (var player in Players)
            {
                player.Hands.Clear();

                int playerBet;
                bool playerBetIsValid;

                Console.Clear();

                do
                {
                    ConsoleUI.WriteColoredLine($"{player.Name}, enter your bet.\n(min: {string.Format("{0:C}", _minBet)}, max: {string.Format("{0:C}", _maxBet)}, must be a whole number)", ConsoleColor.Cyan);
                    ConsoleUI.WriteColoredLine($"Winnings: {string.Format("{0:C}", player.Winnings)}", ConsoleColor.Green);
                    ConsoleUI.TryRead(out playerBet);

                    Console.Clear();

                    playerBetIsValid = (playerBet <= player.Winnings) && (playerBet >= _minBet) && (playerBet <= _maxBet);

                    if (!playerBetIsValid)
                    {
                        ConsoleUI.WriteColoredLine("Invalid bet.\n", ConsoleColor.Red);
                    }
                }
                while (!playerBetIsValid);

                Console.CursorVisible = false;

                player.Winnings -= playerBet;

                Console.Clear();

                Console.WriteLine($"{player.Name} has bet {string.Format("{0:C}", playerBet)}.");
                ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Winnings)} (-{string.Format("{0:C}", playerBet)})", ConsoleColor.Red);

                var hand = new PlayerHand(playerBet);

                hand.DealCard(Dealer.Deck, true);
                hand.DealCard(Dealer.Deck, true);

                player.Hands.Add(hand);

                Thread.Sleep(2000);
            }
        }

        static void ResolvePlayerTurns()
        {
            foreach (Player player in Players)
            {
                for (int i = 0; i < player.Hands.Count; i++)
                {
                    // FIXME: #1: Index iterating over player hands is out of range after that player splits a hand
                    var hand = player.Hands[i];

                    if (hand.State == HandState.Blackjack)
                    {
                        continue;
                    }
					
                    while ((hand.State == HandState.Active) || (hand.State == HandState.Split))
                    {
                        Console.Clear();

                        if (hand.Cards.Count == 1)
                        {
                            hand.DealCard(Dealer.Deck, true);
                        }

                        DisplayHands(hand, player);

                        Option[] turnOptions = hand.GetTurnOptions(player);

                        Option selectedTurnOption = DisplayMenu(turnOptions, 0, 10);

                        selectedTurnOption.Action(player);

                        if (hand.State == HandState.Split)
                        {
                            i--;

                            Thread.Sleep(2000);
                        }
                    }

                    Console.Clear();

                    DisplayHands(hand, player);

                    Thread.Sleep(2000);
                }
            }
        }
    }

    public static void DisplayHands(PlayerHand hand, Player owner)
    {
        ConsoleColor regularPlayerHandColor = ConsoleColor.Magenta;
        ConsoleColor bustedPlayerHandColor = ConsoleColor.Red;

        ConsoleColor playerColor = (hand.State == HandState.Busted) ? bustedPlayerHandColor : regularPlayerHandColor;

        Console.WriteLine(Dealer.Hand.DisplayCards());
        ConsoleUI.WriteColoredLine(Dealer.Hand.Value, ConsoleColor.Magenta);

        Console.WriteLine($"\n{hand.DisplayCards()}");
        ConsoleUI.WriteColoredLine(hand.Value, playerColor);
        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

        ConsoleUI.WriteColoredLine($"\n{owner.Name}", ConsoleColor.Cyan);
        ConsoleUI.WriteColoredLine(string.Format("{0:C}", owner.Winnings), ConsoleColor.DarkGray);

        if (hand.State == HandState.Busted)
        {
            ConsoleUI.WriteColoredLine("\nBust", ConsoleColor.Red);
        
            hand.Bet = 0;
        }
        else if (hand.State == HandState.Surrendered)
        {
            ConsoleUI.WriteColoredLine("\nSurrender!", ConsoleColor.Red);
            ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", owner.Winnings)} (+{string.Format("{0:C}", hand.Bet / 2m)})", ConsoleColor.Red);

            hand.Bet = 0;
        }
    }

    public static Option DisplayMenu(this IEnumerable<Option> options, int left, int top)
    {
        var optionNames = options.Select(option => option.Name);
        
        int selectedOptionIndex = ConsoleUI.DisplayMenu(optionNames, left, top);

        return options.ElementAt(selectedOptionIndex);
    }
}