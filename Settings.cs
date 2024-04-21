namespace Twksqr.Blackjack;

internal static class Settings
{
    internal static readonly int MaxRounds = 0;
    internal static readonly bool PlayersCanLeaveBeforeLastRound = false;

    internal static readonly int MinPlayers = 1;
    internal static readonly int MaxPlayers = 7;

    internal static decimal InitialWinnings { get; } = 100m;

    internal static readonly int MinBet = 5;
    internal static readonly int MaxBet = 50;

    internal static bool DoubledDownCardsAreHidden { get; } = true;

    internal static void InsertStartingCards(this IList<Card> deck, IEnumerable<int> cardValues)
    {
        foreach (int cardValue in cardValues)
        {
            deck.Insert(0, new(cardValue, Suit.Spades));
        }
    }
}