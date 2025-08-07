namespace Waystone.Monads.Iterators.Adapters;

using System.Collections.Generic;
using Extensions;
using Options;

/// <summary>
/// An <see cref="Iter{T}" /> that takes a specified number of elements
/// from the beginning of a sequence.
/// </summary>
/// <typeparam name="T">
/// The type of elements in the sequence. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class Take<T> : Iter<T> where T : notnull
{
    /// <inheritdoc />
    public Take(IEnumerable<T> elements, int count) : this(
        elements.IntoIter(),
        count)
    { }

    /// <inheritdoc />
    internal Take(Iter<T> iter, int count) : base(ExecuteTake(iter, count))
    { }

    private static IEnumerable<T> ExecuteTake(Iter<T> iter, int count)
    {
        for (Option<T> item = iter.Next(); item.IsSome; item = iter.Next())
        {
            if (count <= 0) yield break;
            if (item.IsNone) yield break;
            count--;
            yield return item.Unwrap();
        }
    }
}
