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

    public new void UpdateValue()
    {
        Value = Cards.Sum(card => card.Value);

        Card? aceWorthOne = Cards.FirstOrDefault(card => card.Value == 1);

        if ((aceWorthOne != null) && (Value <= 11))
        {
            aceWorthOne.SetValue(11);
            UpdateValue();
        }
        else if (Value > 21)
        {
            Card? aceWorthEleven = Cards.FirstOrDefault(card => card.Value == 11);

            if (aceWorthEleven == null)
            {
                State = HandState.Busted;
                return;
            }

            aceWorthEleven.SetValue(1);
            UpdateValue();
        }
        else if ((Value == 21) && (Cards.Count == 2) && (State != HandState.Split))
        {
            State = HandState.Blackjack;
        }
    }

    public Option[] GetTurnOptions(Player owner)
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

        return turnOptions.ToArray();
    }

    private void Hit(Player owner)
    {
        this.DealCard(Dealer.Deck, true);
    }

    private void DoubleDown(Player owner)
    {
        owner.Winnings -= Bet;
        Bet *= 2;

        this.DealCard(Dealer.Deck, !GameManager.DoubledDownCardsAreHidden);

        State = HandState.DoubledDown;
    }

    private void Split(Player owner)
    {
        owner.Winnings -= Bet;

        var newHand = new PlayerHand(Bet);
        newHand.Cards.Add(Cards[1]);
        Cards.RemoveAt(1);

        State = HandState.Split;

        owner.Hands.Add(newHand);
    }

    public void Stand(Player owner)
    {
        State = HandState.Stood;
    }

    private void Surrender(Player owner)
    {
        owner.Winnings += Bet / 2m;

        State = HandState.Surrendered;
    }

    public Option[] GetInsuranceOptions(Player player)
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