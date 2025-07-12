namespace Waystone.Monads.Iterators.Abstractions;

/// <summary>
/// Represents an interface for converting an object into an iterator of
/// items of type <typeparamref name="TItem" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of items to iterate over. Must be a
/// non-nullable type.
/// </typeparam>
public interface IIntoIterator<TItem>
    where TItem : notnull
{
    /// <summary>
    /// Converts an object implementing this interface into an iterator that
    /// can iterate over items of type <typeparamref name="TItem" />.
    /// </summary>
    /// <returns>An instance of <see cref="IIterator{TItem}" /></returns>
    IIterator<TItem> IntoIter();
}
