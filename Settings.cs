namespace Twksqr.Blackjack;

internal static class Settings
{
    internal static readonly int maxRounds = 0;
    internal static readonly bool playersCanLeaveBeforeLastRound = false;

    internal static readonly int minPlayers = 1;
    internal static readonly int maxPlayers = 7;

    internal static readonly decimal initialWinnings = 100m;

    internal static readonly int minBet = 5;
    internal static readonly int maxBet = 50;

    internal static readonly bool doubledDownCardsAreHidden = true;
}