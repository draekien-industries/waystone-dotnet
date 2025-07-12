namespace Waystone.Monads.Iterators.Abstractions;

/// <summary>
/// Represents an interface that provides functionality to append or
/// extend an existing collection with the items from another iterator. The generic
/// constraint ensures that the item type is non-nullable.
/// </summary>
/// <typeparam name="TItem">
/// The type of the item to extend or iterate over. Must be
/// a non-nullable type.
/// </typeparam>
public interface IExtend<TItem> where TItem : notnull
{
    /// <summary>Extends an existing collection with the items from another iterator.</summary>
    /// <param name="iter">
    /// The source that will be converted into an iterator to
    /// provide the items for extension.
    /// </param>
    void Extend(IIntoIterator<TItem> iter);
}
