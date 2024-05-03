namespace Twksqr.Blackjack;

public sealed class PlayerHand : Hand
{
    public decimal Bet { get; private set; }
    public decimal InsuranceBet { get; private set; }

    public PlayerHand(decimal bet)
    {
        Bet = bet;

        if (Settings.FiveCardCharlieIsEnabled)
        {
            _cards.CollectionChanged += CheckFiveChardCharlie;
        }
    }

    public IEnumerable<Option> GetTurnOptions(Player owner)
    {
        var turnOptions = new List<Option>()
        {
            new("Hit", Hit),
            new("Stand", Stand)
        };

        if (_cards.Count == 2)
        {
            if (owner.Winnings >= Bet)
            {
                turnOptions.Add(new Option("Double Down", DoubleDown));

                if (((_cards[0].Rank == _cards[1].Rank) || (_cards[0].Value == _cards[1].Value)) && (owner.Hands.Count < 4))
                {
                    turnOptions.Add(new Option("Split", Split));
                }
            }

            turnOptions.Add(new Option("Surrender", Surrender));
        }

        return turnOptions;

        void Hit(Player owner)
        {
            DealCard(Dealer.Deck, true);
        }

        void Stand(Player owner)
        {
            Status = HandStatus.Stood;
        }

        void DoubleDown(Player owner)
        {
            owner.Winnings -= Bet;
            Bet *= 2;

            DealCard(Dealer.Deck, !Settings.DoubledDownCardsAreHidden);

            Status = HandStatus.DoubledDown;
        }

        void Split(Player owner)
        {   
            owner.Winnings -= Bet;

            var newHand = new PlayerHand(Bet);

            newHand.DealCard(_cards, true);

            Status = HandStatus.Split;
            newHand.Status = HandStatus.Split;

            owner.Hands.Add(newHand);
        }

        void Surrender(Player owner)
        {
            owner.Winnings += Bet / 2m;

            Status = HandStatus.Surrendered;
        }
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

    public void CheckFiveChardCharlie(object? sender, EventArgs e)
    {
        if ((Count >= 5) && (Value <= 21))
        {
            Status = HandStatus.FiveCardCharlie;
        }
    }
}