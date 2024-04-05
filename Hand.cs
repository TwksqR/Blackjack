namespace Blackjack;

using System.Collections.ObjectModel;
using System.Collections.Specialized;

public class Hand
{
    public ObservableCollection<Card> Cards { get; } = new();
    
    public int Value { get; set; }

    public string DisplayValue { get; private set; } = "?";

    public bool IsBusted { get; set; } = false;

    public bool IsBlackjack { get; protected set; } = false; // Important for comparing hand values, where a Blackjack beats any other hand 

    public Hand()
    {
        Cards.CollectionChanged += UpdateValue;
    }

    protected virtual void UpdateValue(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Value = Cards.Sum(card => card.Value);

        var aceWorthOne = Cards.FirstOrDefault(card => card.Value == 1);

        if ((aceWorthOne != null) && (Value <= 10))
        {
            aceWorthOne.SetValue(11);
            Value += 10; // The value increases from 1 to 11, a difference of 10
        }

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
                Value -= 10; // The value decreases from 11 to 1, a difference of 10   
            }
        }
        else if (Cards.Count == 2)
        {
            IsBlackjack = true;
        }

        // FIXME: Player hand value is hidden
        DisplayValue = (Cards.All(card => card.IsFaceUp)) ? Value.ToString() : "HUH";
    }

    public string DisplayCards()
    {
        return string.Join(' ', Cards.Select(card => card.ShortName));
    }
}