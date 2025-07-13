namespace Waystone.Monads.Iterators.Abstractions;

using System.Collections.Generic;

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
public interface IFromIterator<in TItem, out TSelf>
    where TItem : notnull
{
    /// <summary>
    /// Creates an instance of the implementing type from the provided
    /// iterator of items.
    /// </summary>
    /// <param name="iter">
    /// The iterator instance that will be used to provide items of
    /// type <typeparamref name="TItem" />.
    /// </param>
    /// <returns>
    /// An instance of the implementing type created using the provided
    /// iterator.
    /// </returns>
    TSelf FromIter(IEnumerable<TItem> iter);
}
