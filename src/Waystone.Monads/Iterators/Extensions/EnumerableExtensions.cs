namespace Waystone.Monads.Iterators.Extensions;

using System.Collections.Generic;
using Options;

/// <summary>
/// Extension methods for converting an <see cref="IEnumerable{T}" /> to
/// an <see cref="Iter{T}" />.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Converts an <see cref="IEnumerable{T}" /> to an <see cref="Iter{T}" />
    /// .
    /// </summary>
    /// <param name="items">The <see cref="IEnumerable{T}" /> to convert.</param>
    /// <typeparam name="T">
    /// The type of items in the <see cref="IEnumerable{T}" />.
    /// Must be a non-nullable type.
    /// </typeparam>
    /// <returns>An <see cref="Iter{T}" /> containing the items from the</returns>
    public static Iter<T> IntoIter<T>(this IEnumerable<T> items)
        where T : notnull => new(items);

    /// <summary>
    /// Converts an <see cref="IEnumerable{T}" /> of <see cref="Option{T}" />
    /// to an <see cref="Iter{T}" /> where each item is unwrapped from an
    /// <see cref="Option{T}" />.
    /// </summary>
    /// <param name="items">
    /// The <see cref="IEnumerable{T}" /> of
    /// <see cref="Option{T}" /> to convert.
    /// </param>
    /// <typeparam name="T">The type of items in the <see cref="IEnumerable{T}" />.</typeparam>
    /// <returns>
    /// An <see cref="Iter{T}" /> containing the unwrapped items from the
    /// <see cref="IEnumerable{T}" /> of <see cref="Option{T}" />. Each item is
    /// included only if it is a <see cref="Some{T}" />; if it is a
    /// <see cref="None{T}" />, it is excluded from the resulting
    /// <see cref="Iter{T}" />.
    /// </returns>
    public static Iter<T> IntoIter<T>(this IEnumerable<Option<T>> items)
        where T : notnull => new(items);
}
