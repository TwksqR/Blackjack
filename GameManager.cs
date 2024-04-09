namespace Twksqr.Blackjack;

using TwksqR;

public static class GameManager
{
    private static readonly int _maxRounds = 2;
    private static readonly bool _playersCanLeaveBeforeMaxRoundReached = false;

    public static List<Player> Players { get; private set; } = new();

    private static readonly int _minPlayers = 1;
    private static readonly int _maxPlayers = 7;

    public static decimal InitialWinnings { get; } = 100m;

    private static readonly int _minBet = 5;
    private static readonly int _maxBet = 50;

    public static bool DoubledDownCardsAreHidden { get; } = true;

    public static void StartGame()
    {
        Console.Clear();
        Console.CursorVisible = false;

        ConsoleUI.WriteColoredLine("Blackjack Simulator", ConsoleColor.Magenta);
        ConsoleUI.WriteColoredLine("\nMade by TwksqR\nDiscord: twskqr\nReddit: u/GD_Stalker", ConsoleColor.DarkGray);

        ConsoleUI.DisplayButton("Let's play!");

        int playerCount = GetPlayerCount();

        Console.CursorVisible = false;

        GetPlayerNames(playerCount);

        PlayGame();

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
    }

    public static void PlayGame()
    {
        int roundNumber = 0;

        // NOTE: Comment to play with unshuffled deck
        Dealer.ShuffleDeck(Dealer.Deck);

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
                    ConsoleUI.WriteColoredLine($"{player.Name} has finished with {string.Format("{0:C}", player.Winnings)}!", ConsoleColor.Green);

                    ConsoleUI.DisplayPressEnter();
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

        ResolvePlayerHands();

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

                        ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);

                        Thread.Sleep(2000);

                        hand.Cards[^1].IsFaceUp = true;
                        hand.UpdateValue();

                        if (hand.IsBusted)
                        {
                            playerColor = ConsoleColor.Red;
                        }

                        Console.Clear();

                        ConsoleUI.WriteColoredLine("Revealing card...", ConsoleColor.Cyan);

                        Console.WriteLine($"\n{hand.DisplayCards()}");
                        ConsoleUI.WriteColoredLine(hand.Value, playerColor);
                        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

                        ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);

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

                    Console.WriteLine($"\n{Dealer.Hand.DisplayCards()}");
                    ConsoleUI.WriteColoredLine(Dealer.Hand.Value, dealerColor);

                    Console.WriteLine($"\n{hand.DisplayCards()}");
                    ConsoleUI.WriteColoredLine(hand.Value, playerColor);
                    ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

                    ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
                    ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Winnings)} (+{string.Format("{0:C}", hand.Bet)})", playerColor);

                    ConsoleUI.WriteColoredLine($"\n{playerHandResult}", playerColor);

                    ConsoleUI.DisplayPressEnter();
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

        static void ResolvePlayerHands()
        {
            foreach (Player player in Players)
            {
                for (int i = 0; i < player.Hands.Count; i++)
                {
                    // FIXME: #1: Index iterating over player hands is out of range after that player splits a hand
                    var hand = player.Hands[i];

<<<<<<< Updated upstream
                    do
=======
                    if (hand.IsBlackjack)
                    {
                        continue;
                    }

                    while (!hand.IsResolved && !hand.IsBusted)
>>>>>>> Stashed changes
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
        }
    }

    public static void DisplayHands(PlayerHand hand, Player owner)
    {
        ConsoleColor regularPlayerHandColor = ConsoleColor.Magenta;
        ConsoleColor bustedPlayerHandColor = ConsoleColor.Red;

        ConsoleColor playerColor = (hand.IsBusted) ? bustedPlayerHandColor : regularPlayerHandColor;

        Console.WriteLine(Dealer.Hand.DisplayCards());
        ConsoleUI.WriteColoredLine(Dealer.Hand.Value, ConsoleColor.Magenta);

        Console.WriteLine($"\n{hand.DisplayCards()}");
        ConsoleUI.WriteColoredLine(hand.Value, playerColor);
        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

        ConsoleUI.WriteColoredLine($"\n{owner.Name}", ConsoleColor.Cyan);
        ConsoleUI.WriteColoredLine(string.Format("{0:C}", owner.Winnings), ConsoleColor.DarkGray);

        if (hand.IsBusted)
        {
            ConsoleUI.WriteColoredLine("\nBust", ConsoleColor.Red);
            ConsoleUI.WriteColoredLine(string.Format("{0:C}", owner.Winnings), ConsoleColor.Red);

            hand.Bet = 0;

            hand.IsResolved = true;
        }
        else if (hand.IsSurrendered)
        {
            ConsoleUI.WriteColoredLine("\nSurrender!", ConsoleColor.Red);
            ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", owner.Winnings)} (+{string.Format("{0:C}", hand.Bet / 2m)})", ConsoleColor.Red);

            hand.Bet = 0;

            hand.IsResolved = true;
        }
    }

    public static Option DisplayMenu(this IEnumerable<Option> options, int left, int top)
    {
        var optionNames = options.Select(option => option.Name);
        
        int selectedOptionIndex = ConsoleUI.DisplayMenu(optionNames, left, top);

        return options.ElementAt(selectedOptionIndex);
    }
}