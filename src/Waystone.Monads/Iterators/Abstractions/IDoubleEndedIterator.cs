namespace Waystone.Monads.Iterators.Abstractions;

using Options;

/// <summary>
/// Represents an interface for a double-ended iterator, which allows
/// iteration over a collection of items of type <typeparamref name="TItem" /> from
/// both the front and the back.
/// </summary>
/// <typeparam name="TItem">
/// The type of the item to iterate over. Must be a
/// non-nullable type.
/// </typeparam>
public interface IDoubleEndedIterator<TItem> : IIterator<TItem>
    where TItem : notnull
{
    /// <summary>
    /// Gets the current position of the iterator relative to the end of the
    /// collection. This property tracks how far the iterator is from the last item in
    /// the collection.
    /// </summary>
    int PositionFromEnd { get; }

    /// <summary>
    /// Retrieves the next item from the back of the double-ended iterator, if
    /// available.
    /// </summary>
    /// <returns>
    /// An <see cref="Option{T}" /> containing the next item from the back if
    /// present, or an empty <see cref="Option{T}" /> if the iterator has reached the
    /// starting point.
    /// </returns>
    Option<TItem> NextBack();
}
