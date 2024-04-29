namespace Twksqr.Blackjack;

using System.Collections.ObjectModel;
using System.ComponentModel;

// https://stackoverflow.com/a/54565283
public class Hand
{
    protected readonly ObservableCollection<Card> _cards = new();
    
    private int _value;
    private int _visibleValue;
    public int Value
    {
        get
        {
            return (AllCardsAreFaceUp) ? _value : _visibleValue;
        }
         
        private set
        {
            _value = value;
        }
    }

    public bool AllCardsAreFaceUp { get; private set; } = true;

    public HandStatus Status { get; protected set; } = HandStatus.Active;

    public event PropertyChangedEventHandler CardPropertyChanged;

    public Hand()
    {
        _cards.CollectionChanged += UpdateValue;
        CardPropertyChanged += CheckAllCardsAreFaceUp;
    }

    protected virtual void UpdateValue(object? sender, EventArgs e)
    {
        Value = GetUpdatedValue(_cards);
        _visibleValue = GetUpdatedValue(_cards.Where(card => card.IsFaceUp));
    }

    protected virtual int GetUpdatedValue(IEnumerable<Card> cards)
    {
        int value = cards.Sum(card => card.Value);

        if (value <= 11)
        {
            if (!cards.Any(card => card.Value == 1))
            {
                return value;
            }

            value += 10;
        }
        else if (value > 21)
        {
            Status = HandStatus.Busted;
            return value;
        }

        if (value == 21)
        {
            Status = ((cards.Count() == 2) && (Status != HandStatus.Split)) ? HandStatus.Blackjack : HandStatus.Stood;
        }

        return value;
    }

    // Erroneous method name, but what else is there?
    private void CheckAllCardsAreFaceUp(object? sender, EventArgs e)
    {
        AllCardsAreFaceUp = _cards.All(card => card.IsFaceUp);
    }

    public void DealCard(IList<Card> oldCollection, bool dealtCardIsFaceUp)
    {
        Card dealtCard = oldCollection[^1];
        oldCollection.Remove(dealtCard);

        dealtCard.IsFaceUp = dealtCardIsFaceUp;

        Add(dealtCard);
    }

    public string GetCardShortNames()
    {
        return string.Join(' ', _cards.Select(card => card.ShortName));
    }

    private void NotifyCardPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        CardPropertyChanged?.Invoke(sender, e);
    }

    public void Add(Card item)
    {
        item.PropertyChanged += NotifyCardPropertyChanged;
        _cards.Add(item);
    }

    public void Clear()
    {
        foreach (var card in _cards)
        {
            card.PropertyChanged -= NotifyCardPropertyChanged;
        }

        _cards.Clear();

        Status = HandStatus.Active;
    }

    public int Count => _cards.Count;

    public bool Contains(Card item)
    {
        return _cards.Contains(item);
    }

    public bool Remove(Card item)
    {
        item.PropertyChanged -= NotifyCardPropertyChanged;
        return _cards.Remove(item);
    }

    public int IndexOf(Card item)
    {
        return _cards.IndexOf(item);
    }

    public void Insert(int index, Card item)
    {
        item.PropertyChanged += NotifyCardPropertyChanged;
        _cards.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _cards[index].PropertyChanged -= NotifyCardPropertyChanged;
        _cards.RemoveAt(index);
    }

    public Card this[int index]
    {
        get => _cards[index];
        set => _cards[index] = value;
    }
}