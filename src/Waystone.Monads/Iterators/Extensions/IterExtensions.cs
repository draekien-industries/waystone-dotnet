namespace Waystone.Monads.Iterators.Extensions;

using System;
using System.Collections.Generic;
using Adapters;
using Options;

/// <summary>Extension methods for <see cref="Iter{T}" />.</summary>
public static class IterExtensions
{
    /// <summary>Creates an <see cref="Iter{T}" /> that clones all of its elements.</summary>
    /// <param name="iter"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Cloned<T> Cloned<T>(this Iter<T> iter) where T : ICloneable =>
        new(iter);

    /// <summary>Lexicographically compares two <see cref="Iter{T}" /> instances.</summary>
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
    /// <param name="left">The first <see cref="Iter{T}" /> to compare.</param>
    /// <param name="right">The second <see cref="Iter{T}" /> to compare.</param>
    /// <typeparam name="T">
    /// The type of elements in the sequence. Must implement
    /// <see cref="IComparable{T}" />.
    /// </typeparam>
    /// <returns>
    /// An <see cref="Ordering" /> value indicating the lexicographical
    /// ordering of the two sequences.
    /// </returns>
    public static Ordering Compare<T>(this Iter<T> left, Iter<T> right)
        where T : IComparable<T>
    {
        using IEnumerator<Option<T>> leftEnumerator = left.GetEnumerator();
        using IEnumerator<Option<T>> rightEnumerator = right.GetEnumerator();

        while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
        {
            Option<T> leftElement = leftEnumerator.Current;
            Option<T> rightElement = rightEnumerator.Current;
        }

        if (leftEnumerator.MoveNext())
        {
            return Ordering.Greater;
        }

        if (rightEnumerator.MoveNext())
        {
            return Ordering.Less;
        }

        return Ordering.Equal;
    }
}
