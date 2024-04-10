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

    public HandState State { get; protected set;} = HandState.Active;

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

        if (Value <= 11)
        {
            Card? aceWorthOne = Cards.FirstOrDefault(card => card.Value == 1);

            if (aceWorthOne == null)
            {
                return;
            }
            
            aceWorthOne.SetValue(11);
            Value += 10;
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
            Value -= 10;
        }
        
        if ((Value == 21) && (Cards.Count == 2) && (State != HandState.Split))
        {
            State = HandState.Blackjack;
        }
    }

    public string DisplayCards()
    {
        return string.Join(' ', Cards.Select(card => card.ShortName));
    }

    public void Reset()
    {
        Cards.Clear();
        State = HandState.Active;
    }
}