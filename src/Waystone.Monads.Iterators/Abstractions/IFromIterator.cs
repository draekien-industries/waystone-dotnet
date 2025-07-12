namespace Waystone.Monads.Iterators.Abstractions;

/// <summary>
/// Defines an interface for creating an instance of the implementing type
/// from an iterator of items of type <typeparamref name="TItem" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of the items provided by the iterator. Must be
/// a non-nullable type.
/// </typeparam>
/// <typeparam name="TSelf">
/// The type of the implementing object that will be
/// created using the iterator.
/// </typeparam>
public interface IFromIterator<TItem, out TSelf>
    where TItem : notnull
    where TSelf : IEnumerable<TItem>
{
    /// <summary>
    /// Creates an instance of the implementing type from the provided
    /// iterator of items.
    /// </summary>
    /// <param name="iter">
    /// The iterator instance that will be used to provide items of
    /// type <typeparamref name="TItem" />.
    /// </param>
    /// <typeparam name="TIter">
    /// The type of the iterator that provides items of type
    /// <typeparamref name="TItem" />. Must implement <see cref="IIterator{TItem}" />.
    /// </typeparam>
    /// <typeparam name="TIntoIter">
    /// The type that can be converted into an iterator
    /// providing items of type <typeparamref name="TItem" />. Must implement
    /// <see cref="IIntoIterator{TItem}" />.
    /// </typeparam>
    /// <returns>
    /// An instance of the implementing type created using the provided
    /// iterator.
    /// </returns>
    TSelf FromIter<TIter, TIntoIter>(TIntoIter iter)
        where TIter : IIterator<TItem>
        where TIntoIter : IIntoIterator<TItem>;
}
