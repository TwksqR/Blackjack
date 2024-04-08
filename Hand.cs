namespace Twksqr.Blackjack;

using System.Collections.ObjectModel;
using System.Collections.Specialized;

public class Hand
{
    public ObservableCollection<Card> Cards { get; } = new();
    
    private int _value;
    public int Value
    {
        get
        {
            return _value - Cards.Where(card => !card.IsFaceUp).Sum(card => card.Value);
        }

        set
        {
            _value = value;
        }
    }

    public bool IsBusted { get; set; } = false;

    public bool IsBlackjack { get; protected set; } = false; // Important for comparing hand values, where a Blackjack beats any other hand 

    public Hand()
    {
        Cards.CollectionChanged += UpdateValue;
    }

    // This is ugly but if it works, it works
    protected virtual void UpdateValue(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateValue();
    }

    public void UpdateValue()
    {
        Value = Cards.Sum(card => card.Value);

        var aceWorthOne = Cards.FirstOrDefault(card => card.Value == 1);

        if ((aceWorthOne != null) && (Value <= 11))
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
    }

    public string DisplayCards()
    {
        return string.Join(' ', Cards.Select(card => card.ShortName));
    }
}