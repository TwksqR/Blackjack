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

        set { _value = value; }
    }

    public virtual bool IsBusted { get; protected set; } = false;

    public bool IsBlackjack { get; protected set; } = false;

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
                IsBusted = true;
                return;
            }

            aceWorthEleven.SetValue(1);
            UpdateValue();
        }
        else if ((Value == 21) && (Cards.Count == 2))
        {
            IsBlackjack = true;
        }
    }

    public string DisplayCards()
    {
        return string.Join(' ', Cards.Select(card => card.ShortName));
    }

    public void Reset()
    {
        Cards.Clear();
        IsBusted = false;
        IsBlackjack = false;
    }
}