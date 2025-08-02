namespace Waystone.Monads.Iterators.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using Adapters;
using Results;
using Results.Errors;

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
        where T : IComparable<T> =>
        (Ordering)new IterComparer<T>().Compare(left, right);

    /// <summary>
    /// Collects all elements of an <see cref="Iter{T}" /> that contains
    /// <see cref="Result{TOk,TErr}" /> elements into a <see cref="Result{TOk,TErr}" />
    /// containing a <see cref="List{T}" /> of type <typeparamref name="T" />.
    /// </summary>
    /// <param name="iter">
    /// The <see cref="Iter{T}" /> to collect results from. The
    /// elements must be of type <see cref="Result{T, Error}" />.
    /// </param>
    /// <typeparam name="T">
    /// The type of elements in the sequence. Must be a
    /// non-nullable type.
    /// </typeparam>
    /// <returns>
    /// A <see cref="Result{T, Error}" /> containing a <see cref="List{T}" />
    /// of type <typeparamref name="T" /> if all elements are successful, or the first
    /// error encountered if any element is an error.
    /// </returns>
    public static Result<List<T>, Error> Collect<T>(
        this Iter<Result<T, Error>> iter) where T : notnull
    {
        List<Result<T, Error>> collected =
            iter.Where(x => x.IsSome).Select(x => x.Unwrap()).ToList();
        Result<T, Error>? firstError = collected.FirstOrDefault(x => x.IsErr);

        if (firstError is not null) return firstError.UnwrapErr();

        return collected.Select(x => x.Unwrap()).ToList();
    }

    /// <summary>Transforms an iterator into a collection.</summary>
    /// <param name="iter">The <see cref="Iter{T}" /> to transform into a collection.</param>
    /// <typeparam name="T">The type of elements in the <see cref="Iter{T}" />.</typeparam>
    /// <returns>
    /// An <see cref="List{T}" /> that contains all the elements produced by
    /// the <see cref="Iter{T}" />.
    /// </returns>
    public static List<T> Collect<T>(this Iter<T> iter) where T : notnull =>
        iter.Where(x => x.IsSome).Select(x => x.Unwrap()).ToList();

    /// <summary>
    /// Creates a <see cref="Adapters.Cycle{T}" /> iterator that cycles
    /// through the elements of the provided <see cref="Iter{T}" />. When the end of
    /// the sequence is reached, it starts over from the beginning.
    /// </summary>
    /// <param name="iter">
    /// The <see cref="Iter{T}" /> to cycle through. The elements
    /// must implement <see cref="ICloneable" /> to allow cloning.
    /// </param>
    /// <typeparam name="T">
    /// The type of elements in the sequence. Must implement
    /// <see cref="ICloneable" />.
    /// </typeparam>
    /// <returns>
    /// A <see cref="Adapters.Cycle{T}" /> iterator that cycles through the
    /// elements of the provided <see cref="Iter{T}" />.
    /// </returns>
    public static Cycle<T> Cycle<T>(this Iter<T> iter) where T : ICloneable =>
        new(iter);
}
