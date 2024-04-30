namespace Twksqr.Blackjack;

using System.Collections.ObjectModel;

// https://stackoverflow.com/a/54565283
public class Hand
{
    protected ObservableCollection<Card> _cards = new();
    protected List<Card> _visibleCards = new();
    
    private int _value;
    private int _visibleValue;
    public int Value
    {
        get
        {
            return (_visibleCards.Count == _cards.Count) ? _value : _visibleValue;
        }
    }

    private HandStatus _status;
    private HandStatus _visibleStatus;
    public HandStatus Status
    {
        get
        {
            return (_visibleCards.Count == _cards.Count) ? _status : _visibleStatus;
        }

        protected set
        {
            _status = _visibleStatus = value;
        }
    }

    public event EventHandler CardVisibilityChanged;

    public Hand()
    {
        _cards.CollectionChanged += UpdateHandValues;
        _cards.CollectionChanged += UpdateVisibleHandValues;

        CardVisibilityChanged += UpdateVisibleHandValues;
    }

    protected virtual void UpdateHandValues(object? sender, EventArgs e)
    {
        _value = _cards.Sum(card => card.Value);

        UpdateHandSelectedValues(_cards, _value, _status);
    }

    protected virtual void UpdateVisibleHandValues(object? sender, EventArgs e)
    {
        _visibleCards = _cards.Where(card => card.IsVisible).ToList();

        if (_visibleCards.Count == _cards.Count)
        {
            _visibleValue = _value;
            return;
        }

        UpdateHandSelectedValues(_visibleCards, _visibleValue, _visibleStatus);
    }

    protected virtual void UpdateHandSelectedValues(IEnumerable<Card> cards, int value, HandStatus status)
    {
        if (value <= 11)
        {
            if (!cards.Any(card => card.Value == 1))
            {
                return;
            }

            value += 10;
        }
        else if (value > 21)
        {
            status = HandStatus.Busted;
            return;
        }

        if (value == 21)
        {
            status = ((cards.Count() == 2) && (Status != HandStatus.Split)) ? HandStatus.Blackjack : HandStatus.Stood;
        }
    }

    public void DealCard(IList<Card> oldCollection, bool dealtCardIsVisible)
    {
        Card dealtCard = oldCollection[^1];
        oldCollection.Remove(dealtCard);

        dealtCard.IsVisible = dealtCardIsVisible;

        Add(dealtCard);
    }

    public string GetCardShortNames()
    {
        return string.Join(' ', _cards.Select(card => card.ShortName));
    }

    private void NotifyCardVisibilityChanged(object? sender, EventArgs e)
    {
        CardVisibilityChanged?.Invoke(sender, e);
    }

    public void Add(Card item)
    {
        item.VisibilityChanged += NotifyCardVisibilityChanged;
        _cards.Add(item);
    }

    public void Clear()
    {
        foreach (var card in _cards)
        {
            card.VisibilityChanged -= NotifyCardVisibilityChanged;
        }

        _cards.Clear();

        _status = HandStatus.Active;
        _visibleStatus = HandStatus.Active;
    }

    public int Count => _cards.Count;

    public bool Contains(Card item)
    {
        return _cards.Contains(item);
    }

    public bool Remove(Card item)
    {
        item.VisibilityChanged -= NotifyCardVisibilityChanged;
        return _cards.Remove(item);
    }

    public int IndexOf(Card item)
    {
        return _cards.IndexOf(item);
    }

    public void Insert(int index, Card item)
    {
        item.VisibilityChanged += NotifyCardVisibilityChanged;
        _cards.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _cards[index].VisibilityChanged -= NotifyCardVisibilityChanged;
        _cards.RemoveAt(index);
    }

    public Card this[int index]
    {
        get => _cards[index];
        set => _cards[index] = value;
    }
}