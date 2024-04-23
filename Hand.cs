namespace Twksqr.Blackjack;

using System.Collections.ObjectModel;

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

    public HandStatus Status { get; protected set; } = HandStatus.Active;

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
                Status = HandStatus.Busted;
                return;
            }

            aceWorthEleven.Value = 1;
            Value -= 10;
        }

        if (Value == 21)
        {
            Status = ((Cards.Count == 2) && (Status != HandStatus.Split)) ? HandStatus.Blackjack : HandStatus.Stood;
        }
    }

    public string GetCardShortNames()
    {
        return string.Join(' ', Cards.Select(card => card.ShortName));
    }

    public void Reset()
    {
        Cards.Clear();
        Status = HandStatus.Active;
    }
}