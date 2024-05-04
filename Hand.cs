namespace Twksqr.Blackjack;

// https://stackoverflow.com/a/54565283
public class Hand
{
    protected List<Card> _cards = new();
    protected List<Card> _visibleCards = new();
    
    public int Value { get; protected set; }

    public HandStatus Status { get; protected set; }

    public Hand()
    {
        CardCollectionChanged += UpdateValue;

        CardVisibilityChanged += UpdateValue;
    }

    // public delegate void CardCollectionChangedEventHandler(CardCollectionChangedEventArgs e);

    public event EventHandler CardCollectionChanged; // public event CollectionChangedEventHandler CardCollectionChanged;
    public event EventHandler CardVisibilityChanged; // public event CollectionChangedEventHandler CardVisibilityChanged;

    protected virtual void UpdateValue(object? sender, EventArgs e)
    {
        _visibleCards = _cards.Where(card => card.IsVisible).ToList();
        Value = _visibleCards.Sum(card => card.Value);

        if (Value <= 11)
        {
            if (!_visibleCards.Any(card => card.Value == 1))
            {
                return;
            }

            Value += 10;
        }
        else if (Value > 21)
        {
            Status = HandStatus.Busted;
            return;
        }

        if (Value == 21)
        {
            Status = ((_visibleCards.Count == 2) && (Status != HandStatus.Split)) ? HandStatus.Blackjack : HandStatus.Stood;
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

        Status = HandStatus.Active;

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