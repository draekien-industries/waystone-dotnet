namespace Waystone.Monads.Iterators;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A comparer for <see cref="Iter{T}" /> that compares the elements of
/// two iterators of type <typeparamref name="T" />. The comparison is
/// lexicographical.
/// </summary>
/// <remarks>
/// Lexicographical comparison is an operation with the following semantics:
/// <list type="bullet">
/// <item>Two sequences are compared element by element.</item>
/// <item>
/// The first mismatching element defines which sequence is lexicographically
/// less or greater than the other.
/// </item>
/// <item>
/// If one sequence is a prefix of the other, the shorter sequence is
/// considered lexicographically less.
/// </item>
/// <item>
/// If two sequences have equivalent elements and are of the same length,
/// then the sequences are considered equal.
/// </item>
/// <item>
/// An empty sequence is considered lexicographically less than any non-empty
/// sequence.
/// </item>
/// <item>Two empty sequences are considered equal.</item>
/// </list>
/// </remarks>
/// <typeparam name="T">
/// The type of elements in the sequence. Must implement
/// <see cref="IComparable{T}" />.
/// </typeparam>
public sealed class IterComparer<T> : IComparer<Iter<T>>
    where T : IComparable<T>
{
    /// <inheritdoc />
    public int Compare(Iter<T>? x, Iter<T>? y) =>
        (x, y) switch
        {
            (x: null, y: null) => (int)Ordering.Equal,
            (x: null, y: { Count: > 0 }) => (int)Ordering.Less,
            (x: { Count: > 0 }, y: null) => (int)Ordering.Greater,
            (x: { Count: 0 }, y: { Count: 0 }) => (int)Ordering.Equal,
            (x: { Count: 0 }, y: { Count: > 0 }) => (int)Ordering.Less,
            (x: { Count: > 0 }, y: { Count: 0 }) => (int)Ordering.Greater,
            var _ => (int)CompareElements(x!, y!),
        };

    private static Ordering CompareElements(Iter<T> left, Iter<T> right)
    {
        T[] leftItems = left.Elements.ToArray();
        T[] rightItems = right.Elements.ToArray();

        if (leftItems.Length == rightItems.Length)
        {
            return CompareElements(
                leftItems,
                rightItems,
                leftItems.Length,
                Ordering.Equal);
        }

        if (leftItems.Length < rightItems.Length)
        {
            return CompareElements(
                leftItems,
                rightItems,
                leftItems.Length,
                Ordering.Less);
        }

        return CompareElements(
            leftItems,
            rightItems,
            rightItems.Length,
            Ordering.Greater);
    }

    private static Ordering CompareElements(
        T[] left,
        T[] right,
        int shortestLength,
        Ordering fallback)
    {
        for (var i = 0; i < shortestLength; i++)
        {
            T leftItem = left[i];
            T rightItem = right[i];

            int comparison = leftItem.CompareTo(rightItem);
            if (comparison != 0) return (Ordering)comparison;
        }

        return fallback;
    }
}
