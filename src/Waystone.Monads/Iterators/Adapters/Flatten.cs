namespace Waystone.Monads.Iterators.Adapters;

using System.Collections.Generic;
using System.Linq;
using Extensions;
using Options;

/// <summary>
/// Creates an <see cref="Iter{T}" /> that flattens a sequence of nested
/// sequences into a single sequence of items.
/// </summary>
/// <typeparam name="T">
/// The type of items in the nested sequences. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class Flatten<T> : Iter<T>
    where T : notnull
{
    /// <inheritdoc />
    public Flatten(IEnumerable<IEnumerable<T>> elements) : base(
        ExecuteFlatten(elements.Select(x => x.IntoIter()).IntoIter()))
    { }

    /// <inheritdoc />
    internal Flatten(Iter<IEnumerable<T>> iter) : base(
        ExecuteFlatten(iter.Map(x => x.IntoIter())))
    { }

    /// <inheritdoc />
    internal Flatten(Iter<Iter<T>> iter) : base(
        ExecuteFlatten(iter))
    { }

    private static IEnumerable<T> ExecuteFlatten(Iter<Iter<T>> iter)
    {
        for (Option<Iter<T>> nestedIter = iter.Next();
             nestedIter.IsSome;
             nestedIter = iter.Next())
        {
            if (nestedIter.IsNone) yield break;
            Iter<T> currentNested = nestedIter.Unwrap();

            for (Option<T> item = currentNested.Next();
                 item.IsSome;
                 item = currentNested.Next())
            {
                if (item.IsNone) yield break;
                yield return item.Unwrap();
            }
        }
    }
}
