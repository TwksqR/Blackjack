
namespace Twksqr.Blackjack;

public sealed class PlayerHand : Hand
{
    public decimal Bet { get; set; }
    public decimal InsuranceBet { get; set; }

    public PlayerHand(decimal bet)
    {
        Bet = bet;

        Cards.CollectionChanged += UpdateValue;
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
        this.DealCard(Dealer.Deck, true);
    }

    public void Stand(Player owner)
    {
        State = HandState.Stood;
    }

    private void DoubleDown(Player owner)
    {
        owner.Winnings -= Bet;
        Bet *= 2;

        this.DealCard(Dealer.Deck, !Settings.DoubledDownCardsAreHidden);

        State = HandState.DoubledDown;
    }

    private void Split(Player owner)
    {
        State = HandState.Split;
        
        owner.Winnings -= Bet;

        var newHand = new PlayerHand(Bet);

        newHand.Cards.Add(Cards[1]);
        Cards.RemoveAt(1);

        owner.Hands.Add(newHand);
    }

    private void Surrender(Player owner)
    {
        owner.Winnings += Bet / 2m;

        State = HandState.Surrendered;
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