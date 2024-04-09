using System.Dynamic;

namespace Twksqr.Blackjack;

public sealed class PlayerHand : Hand
{
    public decimal Bet { get; set; }
    public decimal SideBet { get; set; }

    public bool IsDoubledDown = false; // For rotating the card texture when made in Godot

    public bool IsSplit { get; private set; } = false;

    public bool IsSurrendered { get; private set; } = false;

    public bool IsInsured { get; private set; } = false;

    public bool IsResolved { get; set; } = false;

    public PlayerHand(decimal bet)
    {
        Bet = bet;

        Cards.CollectionChanged += UpdateValue;
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

        void Hit(Player owner)
        {
            this.DealCard(Dealer.Deck, true);
        }

        void DoubleDown(Player owner)
        {
            owner.Winnings -= Bet;
            Bet *= 2;

            this.DealCard(Dealer.Deck, !GameManager.DoubledDownCardsAreHidden);

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

    public Option[] GetInsuranceOptions(Player player)
    {
        return new Option[]
        {
            new("Yes", TakeInsurance),
            new("No", IgnoreInsurance)
        };

        void TakeInsurance(Player player)
        {
            SideBet = Bet / 2m;

            player.Winnings -= SideBet;

            IsInsured = true;
        }

        void IgnoreInsurance(Player player) {}
    }
}