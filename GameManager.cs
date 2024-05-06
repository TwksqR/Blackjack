namespace Twksqr.Blackjack;

using TwksqR;

public static class GameManager
{
    private static readonly List<Player> _players = new();

    public static void Execute()
    {
        Console.Clear();
        Console.CursorVisible = false;

        ConsoleUI.WriteColoredLine("Blackjack Simulator", ConsoleColor.Magenta);

        ConsoleUI.WriteColoredLine("\nMade by TwksqR\nDiscord: twskqr\nReddit: u/GD_Stalker", ConsoleColor.Cyan);

        ConsoleUI.DisplayButton("Let's play!");

        StartGame();
    }

    private static void StartGame()
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

                playerCountIsValid = (Settings.MinPlayers <= playerCount) && (playerCount <= Settings.MaxPlayers);
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

                    playerNameIsVerified = !_players.Any(previousPlayer => previousPlayer.Name == playerName);

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

                _players.Add(new Player(playerName));
            }
        }

        int roundNumber = 0;

        Dealer.ShuffleDeck(Dealer.Deck);

        // Dealer.Deck.AddDebugCards();

        do
        {
            roundNumber++;

            PlayRound(roundNumber);

            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].Money < Settings.MinBet)
                {
                    ConsoleUI.WriteColoredLine($"{_players[i].Name} has bust out!", ConsoleColor.Red);

                    _players.RemoveAt(i);

                    i--;

                    Thread.Sleep(2000);
                }
            }

            if (roundNumber == Settings.MaxRounds)
            {
                foreach (var player in _players)
                {
                    Console.Clear();

                    ConsoleUI.WriteColoredLine($"{player.Name} has finished with {string.Format("{0:C}", player.Money)}!", ConsoleColor.Green);

                    ConsoleUI.DisplayButtonPressEnter();
                }

                break;
            }

            if ((Settings.MaxRounds == 0) || Settings.PlayersCanLeaveBeforeLastRound)
            {
                for (int i = 0; i < _players.Count; i++)
                {
                    Console.Clear();

                    ConsoleUI.WriteColoredLine($"{_players[i].Name}, play again?", ConsoleColor.Cyan);
                    ConsoleUI.WriteColoredLine($"Money: {string.Format("{0:C}", _players[i].Money)}", ConsoleColor.Green);

                    var options = new Option[]
                    {
                        new("Play again", PlayAgain),
                        new("Walk out", WalkOut)
                    };

                    static void PlayAgain(Player player) {}

                    static void WalkOut(Player player)
                    {
                        Console.Clear();

                        ConsoleUI.WriteColoredLine($"{player.Name} has walked out with {string.Format("{0:C}", player.Money)}!", ConsoleColor.Green);

                        _players.Remove(player);

                        Thread.Sleep(2000);
                    }

                    var selectedOption = DisplayMenu(options, 0, 3);

                    selectedOption.Action(_players[i]);

                    if (selectedOption.Name == "Walk out")
                    {
                        i--;
                    }
                }
            }
        }
        while (_players.Count > 0);

        Console.Clear();

        ConsoleUI.WriteColoredLine("Thanks for playing!", ConsoleColor.Magenta);

        Thread.Sleep(2000);
    }

    private static void PlayRound(int roundNumber)
    {
        Console.Clear();

        string roundDisplay = (Settings.MaxRounds > 0) ? $"Round {roundNumber}/{Settings.MaxRounds}" : $"Round {roundNumber}";

        ConsoleUI.WriteColoredLine(roundDisplay, ConsoleColor.Magenta);

        Console.WriteLine($"\n{Dealer.Deck.Count} cards remaining in the deck");

        if (Dealer.Deck.Count <= 234)
        {
            Dealer.ReshuffleDeck();
        }

        Thread.Sleep(2000);

        Dealer.Hand.Clear();

        Dealer.Hand.DealCard(Dealer.Deck, false);
        Dealer.Hand.DealCard(Dealer.Deck, true);

        GetPlayerBets();

        Console.CursorVisible = false;

        if ((Dealer.Hand[1].Value >= 10) || (Dealer.Hand[1].Rank == 1)) // A bit crude but it works
        {
            if (Dealer.Hand[1].Rank == 1)
            {
                foreach (var player in _players)
                {
                    var hand = player.Hands[0];

                    Console.Clear();

                    DisplayHands(hand, player);

                    var insuranceOptions = player.Hands[0].GetInsuranceOptions();

                    ConsoleUI.WriteColoredLine("\nInsurance?", ConsoleColor.Cyan);
                    Option selectedInsuranceOption = DisplayMenu(insuranceOptions, 0, 11);

                    selectedInsuranceOption.Action(player);

                    if (selectedInsuranceOption.Name == "Yes")
                    {
                        Console.Clear();

                        DisplayHands(hand, player);

                        Thread.Sleep(2000);
                    }
                }
            }

            Console.Clear();

            ConsoleUI.WriteColoredLine("Checking dealer's hand for Blackjack...", ConsoleColor.Cyan);

            Console.WriteLine($"\n{Dealer.Hand.GetCardShortNames()}");

            Thread.Sleep(2000);

            Console.Clear();

            Dealer.Hand[0].IsVisible = true;

            if (Dealer.Hand.Status == HandStatus.Blackjack)
            {
                Console.WriteLine("Blackjack!");

                Console.WriteLine($"\n{Dealer.Hand.GetCardShortNames()}");
                ConsoleUI.WriteColoredLine(Dealer.Hand.Value, ConsoleColor.Magenta);

                Thread.Sleep(2000);

                foreach (var player in _players)
                {
                    Console.Clear();

                    ConsoleColor winColor = ConsoleColor.Green;
                    ConsoleColor loseColor = ConsoleColor.Red;
                    ConsoleColor tieColor = ConsoleColor.DarkGray;

                    ConsoleColor dealerColor = tieColor;
                    ConsoleColor playerColor = tieColor;

                    var hand = player.Hands[0];

                    string playerHandResult = "Push";

                    decimal betPayout = 0;
                    decimal insuranceBetPayout = 0;
                    decimal profit = 0m;

                    ConsoleColor betColor = ConsoleColor.Red;
                    ConsoleColor insuranceBetColor = ConsoleColor.Red;

                    if (hand.Status == HandStatus.Blackjack)
                    {
                        player.Money += hand.Bet;

                        betPayout = hand.Bet;
                        betColor = ConsoleColor.Green;
                    }
                    else
                    {
                        dealerColor = winColor;
                        playerColor = loseColor;

                        playerHandResult = "Lose";

                        profit -= hand.Bet;
                    }

                    if (hand.InsuranceBet > 0m)
                    {
                        player.Money += hand.InsuranceBet * 3;
                        profit += hand.InsuranceBet * 2;

                        insuranceBetPayout = hand.InsuranceBet * 3;
                        insuranceBetColor = ConsoleColor.Green;
                    }

                    ConsoleColor profitColor = ConsoleColor.DarkGray;

                    if (profit > 0m)
                    {
                        profitColor = ConsoleColor.Green;
                    }
                    else if (profit < 0m)
                    {
                        profitColor = ConsoleColor.Red;
                    }

                    Console.WriteLine($"\n{Dealer.Hand.GetCardShortNames()}");
                    ConsoleUI.WriteColoredLine(Dealer.Hand.Value, dealerColor);

                    Console.WriteLine($"\n{hand.GetCardShortNames()}");
                    ConsoleUI.WriteColoredLine(hand.Value, playerColor);
                    ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

                    if (hand.InsuranceBet > 0m)
                    {
                        ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", hand.InsuranceBet)} insurance", ConsoleColor.DarkGray);
                    }

                    ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
                    ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Money)} ({string.Format("{0:C}", profit)} profit)", profitColor);
                    ConsoleUI.WriteColoredLine(string.Format("{0:C}", betPayout), betColor);

                    if (hand.InsuranceBet > 0m)
                    {
                        ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", insuranceBetPayout)} insurance", insuranceBetColor);
                    }

                    ConsoleUI.WriteColoredLine($"\n{playerHandResult}", playerColor);

                    ConsoleUI.DisplayButtonPressEnter();

                    if (hand.Status == HandStatus.Blackjack)
                    {
                        player.Hands.Remove(hand);
                    }
                }

                return;
            }
            else
            {
                Dealer.Hand[0].IsVisible = false;

                ConsoleUI.WriteColoredLine("Dealer does not have Blackjack.", ConsoleColor.White);

                Console.WriteLine($"\n{Dealer.Hand.GetCardShortNames()}");

                Thread.Sleep(2000);

                foreach (var player in _players)
                {
                    var hand = player.Hands[0];

                    Console.Clear();

                    if (hand.InsuranceBet > 0)
                    {
                        Console.WriteLine($"\n{Dealer.Hand.GetCardShortNames()}");

                        Console.WriteLine($"\n{hand.GetCardShortNames()}");
                        ConsoleUI.WriteColoredLine(hand.Value, ConsoleColor.Magenta);
                        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);
                        ConsoleUI.WriteColoredLine($"(-{string.Format("{0:C}", hand.InsuranceBet)} insurance)", ConsoleColor.Red);

                        ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
                        ConsoleUI.WriteColoredLine(string.Format("{0:C}", player.Money), ConsoleColor.DarkGray);

                        ConsoleUI.DisplayButtonPressEnter();
                    }
                }
            }
        }

        foreach (var player in _players)
        {
            var hand = player.Hands[0];

            if (hand.Status != HandStatus.Blackjack)
            {
                continue;
            }

            player.Money += hand.Bet * 2.5m;

            Console.Clear();

            Console.WriteLine($"\n{Dealer.Hand.GetCardShortNames()}");

            Console.WriteLine($"\n{hand.GetCardShortNames()}");
            ConsoleUI.WriteColoredLine(hand.Value, ConsoleColor.Magenta);
            ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

            ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
            ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Money)} (+{string.Format("{0:C}", hand.Bet)})", ConsoleColor.Green);

            ConsoleUI.WriteColoredLine("\nBlackjack!", ConsoleColor.Green);

            ConsoleUI.DisplayButtonPressEnter();

            player.Hands.Remove(hand);
        }

        ResolvePlayerTurns();

        Console.Clear();

        // All players bust out or surrendered, automatically losing
        // There is no need to resolve the dealer's hand
        if (!_players.Any(player => player.Hands.Any()))
        {
            return;
        }

        Dealer.ResolveHand();

        foreach (var player in _players)
        {
            foreach (var hand in player.Hands)
            {
                DisplayHandSettlement(hand, player);

                ConsoleUI.DisplayButtonPressEnter();
            }
        }

        Console.Clear();

        static void GetPlayerBets()
        {
            foreach (var player in _players)
            {
                player.Hands.Clear();

                int playerBet;
                bool playerBetIsValid;

                Console.Clear();

                do
                {
                    ConsoleUI.WriteColoredLine($"{player.Name}, enter your bet.\n(min: {string.Format("{0:C}", Settings.MinBet)}, max: {string.Format("{0:C}", Settings.MaxBet)}, must be a whole number)", ConsoleColor.Cyan);
                    ConsoleUI.WriteColoredLine($"Money: {string.Format("{0:C}", player.Money)}", ConsoleColor.Green);
                    ConsoleUI.TryRead(out playerBet);

                    Console.Clear();

                    playerBetIsValid = (playerBet <= player.Money) && (playerBet >= Settings.MinBet) && (playerBet <= Settings.MaxBet);

                    if (!playerBetIsValid)
                    {
                        ConsoleUI.WriteColoredLine("Invalid bet.\n", ConsoleColor.Red);
                    }
                }
                while (!playerBetIsValid);

                Console.CursorVisible = false;

                player.Money -= playerBet;

                Console.Clear();

                Console.WriteLine($"{player.Name} has bet {string.Format("{0:C}", playerBet)}.");
                ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Money)} (-{string.Format("{0:C}", playerBet)})", ConsoleColor.Red);

                Thread.Sleep(2000);

                var hand = new PlayerHand(playerBet);

                hand.DealCard(Dealer.Deck, true);
                hand.DealCard(Dealer.Deck, true);

                player.Hands.Add(hand);
            }
        }

        static void ResolvePlayerTurns()
        {
            foreach (Player player in _players)
            {
                for (int i = 0; i < player.Hands.Count; )
                {
                    var hand = player.Hands[i];
					
                    while ((hand.Status == HandStatus.Active) || (hand.Status == HandStatus.Split))
                    {
                        if (hand.Count < 2)
                        {
                            hand.DealCard(Dealer.Deck, true);
                            continue;
                        }

                        Console.Clear();

                        DisplayHands(hand, player);

                        IEnumerable<Option> turnOptions = hand.GetTurnOptions(player);
                        int selectedTurnOptionIndex = ConsoleUI.DisplayMenu(turnOptions.Select(option => option.Name));

                        Option selectedTurnOption = turnOptions.ElementAt(selectedTurnOptionIndex);
                        selectedTurnOption.Action(player);
                    }

                    if (hand.Status == HandStatus.FiveCardCharlie)
                    {
                        player.Money += hand.Bet * 2;

                        Console.Clear();
            
                        Console.WriteLine($"\n{Dealer.Hand.GetCardShortNames()}");
                        ConsoleUI.WriteColoredLine(Dealer.Hand.Value, ConsoleColor.Red);

                        Console.WriteLine($"\n{hand.GetCardShortNames()}");
                        ConsoleUI.WriteColoredLine(hand.Value, ConsoleColor.Green);
                        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

                        ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
                        ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Money)} (+{string.Format("{0:C}", hand.Bet * 2)})", ConsoleColor.Green);

                        ConsoleUI.WriteColoredLine($"\nWin (Five-Card Charlie)", ConsoleColor.Green);

                        ConsoleUI.DisplayButtonPressEnter();

                        player.Hands.Remove(hand);

                        continue;
                    }

                    if ((hand.Status == HandStatus.Stood) || (hand.Status == HandStatus.DoubledDown) || (hand.Status == HandStatus.Blackjack))
                    {
                        i++;
                    }

                    Console.Clear();

                    DisplayHands(hand, player);

                    Thread.Sleep(2000);
                }
            }
        }
    }

    private static void DisplayHands(PlayerHand hand, Player player)
    {
        ConsoleColor playerColor = (hand.Status == HandStatus.Busted) ? ConsoleColor.Red : ConsoleColor.Magenta;

        Console.WriteLine(Dealer.Hand.GetCardShortNames());

        Console.WriteLine($"\n{hand.GetCardShortNames()}");
        ConsoleUI.WriteColoredLine(hand.Value, playerColor);
        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

        if (hand.InsuranceBet > 0m)
        {
            ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", hand.InsuranceBet)} insurance", ConsoleColor.DarkGray);
        }

        ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
        Console.WriteLine(string.Format("{0:C}", player.Money));

        switch (hand.Status)
        {
            case HandStatus.Stood:
                ConsoleUI.WriteColoredLine("\nStand", ConsoleColor.Blue);

                break;

            case HandStatus.Busted:
                ConsoleUI.WriteColoredLine("\nBust", ConsoleColor.Red);
        
                player.Hands.Remove(hand);

                break;

            case HandStatus.Surrendered:
                ConsoleUI.WriteColoredLine("\nSurrender", ConsoleColor.Red);
                ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Money)} (+{string.Format("{0:C}", hand.Bet / 2m)})", ConsoleColor.Red);

                player.Hands.Remove(hand);

                break;
        }
    }

    private static void DisplayHandSettlement(PlayerHand hand, Player player)
    {
        if (Settings.DoubledDownCardsAreHidden && (hand.Status == HandStatus.DoubledDown))
        {
            Console.Clear();

            ConsoleUI.WriteColoredLine($"Revealing {player.Name}'s hand...", ConsoleColor.Cyan);

            Console.WriteLine($"\n{hand.GetCardShortNames()}");
            ConsoleUI.WriteColoredLine(hand.Value, ConsoleColor.Magenta);
            ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

            ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);

            Thread.Sleep(2000);

            hand[^1].IsVisible = true;

            Console.Clear();

            Console.WriteLine($"{player.Name}'s hand revealed.");

            if (hand.Status == HandStatus.Busted)
            {
                Console.WriteLine($"\n{hand.GetCardShortNames()}");
                ConsoleUI.WriteColoredLine(hand.Value, ConsoleColor.Magenta);
                ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

                ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
                ConsoleUI.WriteColoredLine(string.Format("{0:C}", player.Money), ConsoleColor.Red);

                ConsoleUI.WriteColoredLine("\nBust", ConsoleColor.Red);

                return;
            }

            Console.WriteLine($"\n{hand.GetCardShortNames()}");
            ConsoleUI.WriteColoredLine(hand.Value, ConsoleColor.Magenta);
            ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

            ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);

            Thread.Sleep(2000);
        }

        Console.Clear();

        ConsoleColor winColor = ConsoleColor.Green;
        ConsoleColor loseColor = ConsoleColor.Red;
        ConsoleColor tieColor = ConsoleColor.DarkGray;

        ConsoleColor dealerColor = tieColor;
        ConsoleColor playerColor = tieColor;

        string playerHandResult = "Stand off";

        if ((hand.Value > Dealer.Hand.Value) || (Dealer.Hand.Status == HandStatus.Busted))
        {
            dealerColor = loseColor;
            playerColor = winColor;

            playerHandResult = "Win";

            player.Money += hand.Bet * 2;
        }
        else if (hand.Value < Dealer.Hand.Value)
        {
            dealerColor = winColor;
            playerColor = loseColor;

            playerHandResult = "Lose";
        }
        else
        {
            player.Money += hand.Bet;
        }

        Console.WriteLine($"\n{Dealer.Hand.GetCardShortNames()}");
        ConsoleUI.WriteColoredLine(Dealer.Hand.Value, dealerColor);

        Console.WriteLine($"\n{hand.GetCardShortNames()}");
        ConsoleUI.WriteColoredLine(hand.Value, playerColor);
        ConsoleUI.WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

        ConsoleUI.WriteColoredLine($"\n{player.Name}", ConsoleColor.Cyan);
        ConsoleUI.WriteColoredLine($"{string.Format("{0:C}", player.Money)} (+{string.Format("{0:C}", hand.Bet)})", playerColor);

        ConsoleUI.WriteColoredLine($"\n{playerHandResult}", playerColor);
    }

    private static Option DisplayMenu(this IEnumerable<Option> options, int left, int top)
    {
        var optionNames = options.Select(option => option.Name);
        
        int selectedOptionIndex = ConsoleUI.DisplayMenu(optionNames, left, top);

        return options.ElementAt(selectedOptionIndex);
    }
}