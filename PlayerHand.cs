namespace Blackjack;

using System.Collections.Specialized;

public sealed class PlayerHand : Hand
{
    public int Bet { get; set; }

    public bool IsDoubledDown = false; // For rotating the card texture when made in Godot

    public bool IsSplit { get; private set; } = false;

    public bool IsSurrendered = false;

    public bool IsResolved = false;

    public PlayerHand(int bet)
    {
        Bet = bet;

        Cards.CollectionChanged += UpdateValue;
    }

    protected override void UpdateValue(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Value = Cards.Sum(card => card.Value);

        if (Value < 21)
        {
            return;
        }

        if (Value > 21)
        {
            var aceWorthEleven = Cards.FirstOrDefault(card => card.Value == 11);

            if (aceWorthEleven == null)
            {
                IsBusted = true;
            }
            else
            {
                aceWorthEleven.SetValue(1);
                Value -= 10; // The value of an decreases changes from 11 to 1, a difference of 10   
            }
        }
        else if ((!IsSplit) && (Cards.Count == 2))
        {
            // TODO: Add 3:2 payout
            // NOTE: If dealer has natural, do not payout 3:2

            IsBlackjack = true;
        }

        DisplayValue = (Cards.All(card => card.IsFaceUp)) ? Value.ToString() : "?";
    }

    public List<TurnAction> GetTurnActions(Player owner)
    {
        var turnActions = new List<TurnAction>()
        {
            new("Hit", Hit),
            new("Stand", Stand)
        };

        if (Cards.Count == 2)
        {
            if (owner.Winnings >= Bet)
            {
                turnActions.Add(new TurnAction("Double Down", DoubleDown));

                if (((Cards[0].Rank == Cards[1].Rank) || (Cards[0].Value == Cards[1].Value)) && (owner.Hands.Count < 4))
                {
                    turnActions.Add(new TurnAction("Split", Split));
                }
            }

            turnActions.Add(new TurnAction("Surrender", Surrender));
        }

        return turnActions;

        void Hit(Player owner)
        {
            this.DealCard(Dealer.Deck, true);
        }

        void DoubleDown(Player owner)
        {
            owner.Winnings -= Bet;
            Bet *= 2;

            this.DealCard(Dealer.Deck, true);

            IsDoubledDown = true;
            IsResolved = true;
        }

        // TODO: Figure out a way to not skip past player's turn upon splitting
        void Split(Player owner)
        {
            owner.Winnings -= Bet;

            var newHand = new PlayerHand(Bet);
            newHand.Cards.Add(Cards[1]);
            Cards.RemoveAt(1);

            IsSplit = true;

            owner.Hands.Add(newHand);
        }

        void Stand(Player owner)
        {
            IsResolved = true;
        }

        void Surrender(Player owner)
        {
            owner.Winnings += Bet / 2m;

            IsSurrendered = true;
            IsResolved = true;
        }
        
        // NOTE: Separate action; done at the start of a round
        // Add Insurance mechanic
        // static void Insurance(PlayerHand hand, Player owner) {}
    }
}