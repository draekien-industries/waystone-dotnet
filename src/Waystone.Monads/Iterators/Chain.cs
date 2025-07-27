namespace Waystone.Monads.Iterators;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a sequential combination of two
/// <see cref="IEnumerable{T}" /> collections into a single iterator of type
/// <typeparamref name="TItem" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements in the chain. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class Chain<TItem> : Iterator<TItem> where TItem : notnull
{
    /// <inheritdoc />
    public Chain(IEnumerable<TItem> first, IEnumerable<TItem> second) : base(
        first.Concat(second))
    { }
}
