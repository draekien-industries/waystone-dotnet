namespace Waystone.Monads.Iterators.Extensions;

using System.Collections.Generic;
using System.Linq;
using Abstractions;

internal static class IntoIteratorExtensions
{
    public static IIterator<TItem>
        IntoIter<TItem>(this IEnumerable<TItem> iter) where TItem : notnull =>
        new ArrayIterator<TItem>(iter.ToArray());
}
