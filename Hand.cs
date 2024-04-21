namespace Twksqr.Blackjack;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

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

        protected set { _value = value; }
    }

    public HandState State { get; protected set; } = HandState.Active;

    public Hand()
    {
        Cards.CollectionChanged += UpdateValue;
    }

    public void UpdateValue(object? sender, EventArgs e)
    {
        Value = Cards.Sum(card => card.Value);

        if (Value <= 11)
        {
            Card? aceWorthOne = Cards.FirstOrDefault(card => card.Value == 1);

            if (aceWorthOne == null)
            {
                return;
            }
            
            aceWorthOne.Value = 11;
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

            aceWorthEleven.Value = 1;
            Value -= 10;
        }

        if (Value == 21)
        {
            State = ((Cards.Count == 2) && (State != HandState.Split)) ? HandState.Blackjack : HandState.Stood;
        }
    }

    public string GetCardShortNames()
    {
        return string.Join(' ', Cards.Select(card => card.ShortName));
    }

    public void Reset()
    {
        Cards.Clear();
        State = HandState.Active;
    }
}