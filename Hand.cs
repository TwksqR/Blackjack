namespace Twksqr.Blackjack;

// https://stackoverflow.com/a/54565283
public class Hand
{
    protected List<Card> _cards = new();
    protected List<Card> _visibleCards = new();
    
    protected int _value;
    protected int _visibleValue;
    public int Value
    {
        get => _visibleValue;
    }

    protected HandStatus _status;
    protected HandStatus _visibleStatus;
    public HandStatus Status
    {
        get => _visibleStatus;

        protected set => _status = _visibleStatus = value;
    }

    public Hand()
    {
        CardCollectionChanged += UpdateHandValues;
        CardCollectionChanged += UpdateVisibleHandValues;

        CardVisibilityChanged += UpdateVisibleHandValues;
    }

    // public delegate void CardCollectionChangedEventHandler(CardCollectionChangedEventArgs e);

    public event EventHandler CardCollectionChanged; // public event CollectionChangedEventHandler CardCollectionChanged;
    public event EventHandler CardVisibilityChanged; // public event CollectionChangedEventHandler CardVisibilityChanged;

    protected virtual void UpdateHandValues(object? sender, EventArgs e)
    {
        _value = _cards.Sum(card => card.Value);

        UpdateHandSelectedValues(_cards, ref _value, ref _status);
    }

    protected virtual void UpdateVisibleHandValues(object? sender, EventArgs e)
    {
        _visibleCards = _cards.Where(card => card.IsVisible).ToList();
        _visibleValue = _visibleCards.Sum(card => card.Value);

        UpdateHandSelectedValues(_visibleCards, ref _visibleValue, ref _visibleStatus);
    }

    protected virtual void UpdateHandSelectedValues(IEnumerable<Card> cards, ref int value, ref HandStatus status)
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
        if (oldCollection.Count < 1)
        {
            return;
        }
        
        Card dealtCard = oldCollection[^1];
        oldCollection.Remove(dealtCard);

        dealtCard.IsVisible = dealtCardIsVisible;

        Add(dealtCard);
    }

    public void DealCard(Hand oldHand, bool dealtCardIsVisible)
    {
        Card dealtCard = oldHand[^1];
        oldHand.Remove(dealtCard);

        dealtCard.IsVisible = dealtCardIsVisible;

        Add(dealtCard);
    }

    public string GetCardShortNames()
    {
        return string.Join(' ', _cards.Select(card => card.ShortName));
    }

    public void Add(Card card)
    {
        card.VisibilityChanged += NotifyCardVisibilityChanged;
        _cards.Add(card);

        NotifyCardCollectionChanged(this, EventArgs.Empty);
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

        NotifyCardCollectionChanged(this, EventArgs.Empty);
    }

    public bool Contains(Card card)
    {
        return _cards.Contains(card);
    }

    public int Count => _cards.Count;

    public int IndexOf(Card card)
    {
        return _cards.IndexOf(card);
    }

    public void Insert(int index, Card card)
    {
        card.VisibilityChanged += NotifyCardVisibilityChanged;
        _cards.Insert(index, card);

        NotifyCardCollectionChanged(this, EventArgs.Empty);
    }

    public bool Remove(Card card)
    {
        card.VisibilityChanged -= NotifyCardVisibilityChanged;
        bool removeSuccessful = _cards.Remove(card);

        if (removeSuccessful)
        {
            NotifyCardCollectionChanged(this, EventArgs.Empty);
        }

        return removeSuccessful;
    }

    public Card this[int index]
    {
        get => _cards[index];

        set
        {
            _cards[index] = value;
            
            NotifyCardCollectionChanged(this, EventArgs.Empty);
        }
    }

    private void NotifyCardVisibilityChanged(object? sender, EventArgs e)
    {
        CardVisibilityChanged?.Invoke(sender, e);
    }

    private void NotifyCardCollectionChanged(object? sender, EventArgs e)
    {
        CardCollectionChanged?.Invoke(sender, e);
    }
}