namespace Waystone.Monads.Iterators.Abstractions;

/// <summary>An iterator that knows its exact length.</summary>
/// <remarks>
/// When implementing an <see cref="IExactSizeIterator{TItem}" />, the
/// implementation of <c>SizeHint()</c> must return the exact size of the iterator.
/// </remarks>
/// <typeparam name="TItem">
/// The type of the item to iterate over. Must be a
/// non-nullable type.
/// </typeparam>
public interface IExactSizeIterator<TItem> : IIterator<TItem>
    where TItem : notnull
{
    /// <summary>Gets the exact length of the iterator.</summary>
    /// <remarks>
    /// The value returned by the <c>Length</c> property must match the exact
    /// number of items that can be iterated over. For implementing types, this value
    /// must align with the result of <c>SizeHint()</c>.
    /// </remarks>
    /// <value>The total number of items in the iterator as an integer.</value>
    int Length { get; }

    /// <summary>Indicates whether the iterator contains no elements.</summary>
    /// <remarks>
    /// The <c>IsEmpty</c> property returns <c>true</c> if the iterator does
    /// not contain any elements, and <c>false</c> otherwise. Implementations must
    /// ensure that this value is consistent with <c>Length</c>, where <c>IsEmpty</c>
    /// is <c>true</c> if and only if <c>Length</c> is zero.
    /// </remarks>
    /// <value>
    /// <c>true</c> if the iterator contains no elements; otherwise,
    /// <c>false</c>.
    /// </value>
#if NET5_0_OR_GREATER
    bool IsEmpty => Length <= 0;
#else
    bool IsEmpty { get;}
#endif
}
