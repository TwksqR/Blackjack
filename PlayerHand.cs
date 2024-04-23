
namespace Twksqr.Blackjack;

public sealed class PlayerHand : Hand
{
    public decimal Bet { get; set; }
    public decimal InsuranceBet { get; set; }

    public PlayerHand(decimal bet)
    {
        Bet = bet;
    }

    public IEnumerable<Option> GetTurnOptions(Player owner)
    {
        var turnOptions = new List<Option>()
        {
            new("Hit", Hit),
            new("Stand", Stand)
        };

        if (Cards.Count == 2)
        {
            if (owner.Winnings >= Bet)
            {
                turnOptions.Add(new Option("Double Down", DoubleDown));

                if (((Cards[0].Rank == Cards[1].Rank) || (Cards[0].Value == Cards[1].Value)) && (owner.Hands.Count < 4))
                {
                    turnOptions.Add(new Option("Split", Split));
                }
            }

            turnOptions.Add(new Option("Surrender", Surrender));
        }

        return turnOptions;
    }

    private void Hit(Player owner)
    {
        Cards.DealCard(Dealer.Deck, true);
    }

    public void Stand(Player owner)
    {
        Status = HandStatus.Stood;
    }

    private void DoubleDown(Player owner)
    {
        owner.Winnings -= Bet;
        Bet *= 2;

        Cards.DealCard(Dealer.Deck, !Settings.DoubledDownCardsAreHidden);

        Status = HandStatus.DoubledDown;
    }

    private void Split(Player owner)
    {   
        owner.Winnings -= Bet;

        var newHand = new PlayerHand(Bet);

        newHand.Cards.DealCard(Cards, true);

        Status = HandStatus.Split;
        newHand.Status = HandStatus.Split;

        owner.Hands.Add(newHand);
    }

    private void Surrender(Player owner)
    {
        owner.Winnings += Bet / 2m;

        Status = HandStatus.Surrendered;
    }

    public IEnumerable<Option> GetInsuranceOptions()
    {
        return new Option[]
        {
            new("Yes", TakeInsurance),
            new("No", IgnoreInsurance)
        };

        void TakeInsurance(Player player)
        {
            InsuranceBet = Bet / 2m;

            player.Winnings -= InsuranceBet;
        }

        void IgnoreInsurance(Player player) {}
    }
}