namespace Blackjack;

public static class GameManager
{
    public static List<Player> Players { get; private set; } = new();

    public static void Execute()
    {
        Console.Clear();
        Console.CursorVisible = false;

        WriteColoredLine("Blackjack Simulator", ConsoleColor.Magenta);
        WriteColoredLine("\nMade by TwksqR\nDiscord: twskqr\nReddit: u/GD_Stalker)", ConsoleColor.DarkGray);

        DisplayButton("Let's play!", 5);

        int playerCount;
        bool playerCountIsValid;

        do
        {
            Console.Clear();

            TryReadInt("Enter player count:", ConsoleColor.Cyan, out playerCount);

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

                WriteColoredLine($"Enter Player {playerNumber}'s name:", ConsoleColor.Cyan);

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

        Dealer.ShuffleDeck();

        do
        {
            roundNumber++;

            PlayRound(roundNumber);

            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Winnings == 0)
                {
                    WriteColoredLine($"{Players[i].Name} has bust out!", ConsoleColor.Red);

                    Players.RemoveAt(i);

                    i--;

                    Thread.Sleep(2000);
                }
            }

            for (int i = 0; i < Players.Count; i++)
            {
                Console.Clear();

                WriteColoredLine($"{Players[i].Name}, play again?", ConsoleColor.Cyan);
                WriteColoredLine($"Winnings: {string.Format("{0:C}", Players[i].Winnings)}", ConsoleColor.Green);

                var options = new string[]
                {
                    "Yes",
                    "No"
                };

                int selectedOption = DisplayOptionsMenu(options, 3);

                if (selectedOption == 1)
                {
                    Console.Clear();

                    WriteColoredLine($"{Players[i].Name} has walked out with {string.Format("{0:C}", Players[i].Winnings)}!", ConsoleColor.Green);

                    Players.RemoveAt(i);

                    i--;

                    Thread.Sleep(2000);
                }
            }
        }
        while (Players.Count > 0);
    }

    public static void PlayRound(int roundNumber)
    {
        int minimumBet = 1;
        int maximumBet = 50;

        Console.Clear();

        WriteColoredLine($"Round {roundNumber}", ConsoleColor.Magenta);
        
        Console.WriteLine($"\n{Dealer.Deck.Count} cards remaining in the deck");

        if (Dealer.Deck.Count <= 234)
        {
            Dealer.ReshuffleDeck();
        }

        Thread.Sleep(2000);

        Dealer.Hand.Cards.Clear();

        Console.CursorVisible = true;

        foreach (var player in Players)
        {
            player.Hands.Clear();

            int playerBet;
            bool playerBetIsValid;

            do
            {
                Console.Clear();

                WriteColoredLine($"{player.Name}, enter your bet. (min: {string.Format("{0:C}", minimumBet)}, max: {string.Format("{0:C}", maximumBet)}, must be a whole number)", ConsoleColor.Cyan);
                TryReadInt($"Winnings: {string.Format("{0:C}", player.Winnings)}", ConsoleColor.Green, out playerBet);

                playerBetIsValid = (playerBet <= player.Winnings) && (playerBet >= minimumBet) && (playerBet <= maximumBet);
            }
            while (!playerBetIsValid);
            
            Console.CursorVisible = false;
             
            var hand = new PlayerHand(playerBet);

            hand.DealCard(Dealer.Deck, true);
            hand.DealCard(Dealer.Deck, true);

            player.Hands.Add(hand);
            player.Winnings -= playerBet;
        }

        Console.CursorVisible = false;

        Dealer.Hand.DealCard(Dealer.Deck, false);
        Dealer.Hand.DealCard(Dealer.Deck, true);

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

                    List<TurnAction> turnActions = hand.GetTurnActions(player);

                    TurnAction selectedTurnAction = DisplayTurnActionsMenu(turnActions, 9);

                    selectedTurnAction.Selection(player);

                    Console.Clear();

                    DisplayHands(hand, player);

                    if (hand.IsBusted)
                    {
                        WriteColoredLine("\nBust!", ConsoleColor.Red);
                        WriteColoredLine($"{string.Format("{0:C}", player.Winnings)} (-{string.Format("{0:C}", hand.Bet)})", ConsoleColor.Red);

                        hand.Bet = 0;

                        hand.IsResolved = true;
                    }
                    else if (hand.IsSurrendered)
                    {
                        WriteColoredLine("\nSurrender!", ConsoleColor.Red);
                        WriteColoredLine($"{string.Format("{0:C}", player.Winnings)} (-{string.Format("{0:C}", hand.Bet / 2m)})", ConsoleColor.Red);
                    
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

        Console.Clear();

        if (Players.Any(player => player.Hands.Any(hand => !hand.IsBusted && !hand.IsSurrendered)))
        {
            Dealer.ResolveHand();

            foreach (Player player in Players)
            {
                foreach (var hand in player.Hands)
                {
                    if (hand.IsSurrendered || hand.IsBusted)
                    {
                        continue;
                    }

                    Console.Clear();

                    ConsoleColor winningHandColor = ConsoleColor.Green;
                    ConsoleColor losingHandColor = ConsoleColor.Red;
                    ConsoleColor tiedHandColor = ConsoleColor.DarkGray;

                    ConsoleColor dealerHandResultColor = tiedHandColor;
                    ConsoleColor playerHandResultColor = tiedHandColor;
                    
                    string playerHandResult = "Push";

                    if ((hand.Value > Dealer.Hand.Value) || (Dealer.Hand.IsBusted))
                    {
                        dealerHandResultColor = losingHandColor;
                        playerHandResultColor = winningHandColor;

                        playerHandResult = "Win";

                        hand.Bet *= 2;

                        player.Winnings += hand.Bet;
                    }
                    else if (hand.Value < Dealer.Hand.Value)
                    {
                        dealerHandResultColor = winningHandColor;
                        playerHandResultColor = losingHandColor;

                        playerHandResult = "Lose";

                        hand.Bet = 0;
                    }
                    else
                    {
                        player.Winnings += hand.Bet;
                    }

                    Console.WriteLine(Dealer.Hand.DisplayCards());
                    WriteColoredLine(Dealer.Hand.Value, dealerHandResultColor);
                    Console.WriteLine();
                    Console.WriteLine(hand.DisplayCards());
                    WriteColoredLine(hand.Value, playerHandResultColor);
                    Console.WriteLine();
                    WriteColoredLine(player.Name, ConsoleColor.DarkGray);
                    Console.WriteLine();
                    WriteColoredLine(playerHandResult, playerHandResultColor);
                    WriteColoredLine($"{string.Format("{0:C}", player.Winnings)} (+{string.Format("{0:C}", hand.Bet)})", playerHandResultColor);

                    DisplayPressEnter();
                }
            }
        }
    }

    public static void DisplayHands(PlayerHand hand, Player owner)
    {
        ConsoleColor regularPlayerHandColor = ConsoleColor.Magenta;
        ConsoleColor bustedPlayerHandColor = ConsoleColor.Red;

        ConsoleColor playerHandColor = (hand.IsBusted) ? bustedPlayerHandColor : regularPlayerHandColor;

        Console.WriteLine(Dealer.Hand.DisplayCards());
        // WriteColoredLine(Dealer.Hand.DisplayValue, ConsoleColor.Magenta);

        Console.WriteLine();

        Console.WriteLine(hand.DisplayCards());
        WriteColoredLine(hand.DisplayValue, playerHandColor);
        WriteColoredLine(string.Format("{0:C}", hand.Bet), ConsoleColor.DarkGray);

        Console.WriteLine();

        WriteColoredLine(owner.Name, ConsoleColor.DarkGray);
        WriteColoredLine(string.Format("{0:C}", owner.Winnings), ConsoleColor.DarkGray);
    }

    public static bool TryReadInt(string text, out int output)
    {
        Console.CursorVisible = true;

        Console.WriteLine(text);

        string input = Console.ReadLine() ?? "";
        bool isVerified = int.TryParse(input, out output);

        return isVerified;
    }

    public static bool TryReadInt(string text, ConsoleColor textColor, out int output)
    {
        Console.CursorVisible = true;

        WriteColoredLine(text, textColor);

        string input = Console.ReadLine() ?? "";
        bool isVerified = int.TryParse(input, out output);

        return isVerified;
    }

    /*
    public static T ReadLineVerified<T>(string text)
    {
        T output;
        bool isVerified;

        do 
        {
            Console.WriteLine(text);
            string input = Console.ReadLine() ?? "";

            switch (typeof(T).Name)
            {
                case "string":
                    isVerified = true;
                    break;
                    
                case "int":
                    isVerified = int.TryParse(input, out output); // Cannot convert
                    break;
            }     
        }
        while (!isVerified); // Unassigned?

        return output; // Unassigned?
    }
    */

    public static TurnAction DisplayTurnActionsMenu(IEnumerable<TurnAction> turnActions, int offset)
    {
        int selectedOptionIndex = 0;

        ConsoleKeyInfo keyInfo;

        string[] turnActionNames = turnActions.Select(turnAction => turnAction.Name).ToArray();

        do
        {
            WriteOptions(turnActionNames, selectedOptionIndex, offset);

            keyInfo = Console.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow: 
                    selectedOptionIndex--;
                    break;

                case ConsoleKey.DownArrow: 
                    selectedOptionIndex++;
                    break;
            }

            selectedOptionIndex = Math.Clamp(selectedOptionIndex, 0, turnActionNames.Length - 1);
        }
        while (keyInfo.Key != ConsoleKey.Enter);

        return turnActions.ElementAt(selectedOptionIndex);
    }

    public static int DisplayOptionsMenu(IEnumerable<string> options, int offset)
    {
        string[] optionsArray = options.ToArray();

        int selectedOptionIndex = 0;

        ConsoleKeyInfo keyInfo;

        do
        {
            WriteOptions(optionsArray, selectedOptionIndex, offset);

            keyInfo = Console.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow: 
                    selectedOptionIndex--;
                    break;

                case ConsoleKey.DownArrow: 
                    selectedOptionIndex++;
                    break;
            }

            selectedOptionIndex = Math.Clamp(selectedOptionIndex, 0, optionsArray.Count() - 1);
        }
        while (keyInfo.Key != ConsoleKey.Enter);

        return selectedOptionIndex;
    }

    private static void WriteOptions(IEnumerable<string> options, int selectedOptionIndex, int offset)
    {
        Console.SetCursorPosition(0, offset);

        for (int i = 0; i < options.Count(); i++)
        {
            var optionColor = (i == selectedOptionIndex) ? ConsoleColor.Yellow : ConsoleColor.DarkBlue;

            WriteColoredLine(options.ElementAt(i), optionColor);
        }
    }

    public static void WriteColoredLine(object line, ConsoleColor lineColor)
    {
        ConsoleColor currentForegroundColor = Console.ForegroundColor;
        
        Console.ForegroundColor = lineColor;

        Console.WriteLine(line);

        Console.ForegroundColor = currentForegroundColor;
    }

    public static void DisplayPressEnter()
    {
        DisplayButton("Press [Enter] to continue");
    }

    public static void DisplayButton(string button)
    {
        WriteColoredLine("\n" + button, ConsoleColor.Yellow);
        while (Console.ReadKey().Key != ConsoleKey.Enter) {}
    }

    public static void DisplayButton(string button, int offset)
    {
        Console.SetCursorPosition(0, offset);

        WriteColoredLine("\n" + button, ConsoleColor.Yellow);
        while (Console.ReadKey().Key != ConsoleKey.Enter) {}
    }
}