namespace Twksqr.Blackjack;

using System.Collections.Specialized;

public sealed class PlayerHand : Hand
{
    public int Bet { get; set; }

    public string DisplayValue { get; private set; } = "?";

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

        var aceWorthOne = Cards.FirstOrDefault(card => card.Value == 1);

        if ((aceWorthOne != null) && (Value <= 10))
        {
            aceWorthOne.SetValue(11);
            Value += 10; // The value increases from 1 to 11, a difference of 10
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
                Value -= 10; // The value decreases from 11 to 1, a difference of 10   
            }
        }
        else if ((Value == 21) && (Cards.Count == 2) && (!IsSplit))
        {
            IsBlackjack = true;
        }

        if (Cards[^1].IsFaceUp)
        {
            DisplayValue = Cards.Where(card => card.IsFaceUp).Sum(card => card.Value).ToString();
        }
    }

    public List<Option> GetTurnOptions(Player owner)
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
    }
}