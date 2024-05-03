namespace Twksqr.Blackjack;

public class CardCollectionChangedEventArgs : EventArgs
{
    public List<Card> ChangedCards { get; }
    
    public CardCollectionChange Change { get; }

    public CardCollectionChangedEventArgs(IEnumerable<Card> changedCards, CardCollectionChange change)
    {
        ChangedCards = changedCards.ToList();
        Change = change;
    }
}

public enum CardCollectionChange
{
    Add,
    Remove,
    CardProperty
}