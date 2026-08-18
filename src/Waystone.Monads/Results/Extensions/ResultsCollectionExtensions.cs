namespace Waystone.Monads.Results.Extensions;

using System.Collections.Generic;
using System.Linq;

/// <summary>Extensions for <see cref="Result{TOk,TErr}" /> collections.</summary>
public static class ResultsCollectionExtensions
{
    /// <summary>
    /// Returns the values contained by the <see cref="Ok{TOk,TErr}" /> elements of a
    /// sequence, skipping the <see cref="Err{TOk,TErr}" /> ones.
    /// </summary>
    /// <remarks>
    /// This is the sequence counterpart of Rust's <c>iter().flatten()</c>, which
    /// works because <c>Result</c> is itself iterable. It is lazy and streams.
    /// Unlike collecting into a single result, it does not stop at the first
    /// failure.
    /// </remarks>
    /// <param name="results">
    /// An <see cref="IEnumerable{T}" /> of
    /// <see cref="Result{TOk,TErr}" />
    /// </param>
    /// <typeparam name="TOk">The result's ok value type</typeparam>
    /// <typeparam name="TErr">The result's error value type</typeparam>
    /// <returns>
    /// An <see cref="IEnumerable{T}" /> yielding the value of every
    /// <see cref="Ok{TOk,TErr}" /> in the source, in order
    /// </returns>
    public static IEnumerable<TOk> Flatten<TOk, TErr>(
        this IEnumerable<Result<TOk, TErr>> results)
        where TOk : notnull where TErr : notnull =>
        results.SelectMany(result => result.AsEnumerable());

    /// <summary>
    /// Returns the errors contained by the <see cref="Err{TOk,TErr}" /> elements of
    /// a sequence, skipping the <see cref="Ok{TOk,TErr}" /> ones.
    /// </summary>
    /// <remarks>
    /// Rust has no name for this direction. The <c>Err</c> suffix follows
    /// <c>MapErr</c>, <c>InspectErr</c>, <c>ExpectErr</c> and <c>UnwrapErr</c>. It is
    /// lazy and streams.
    /// </remarks>
    /// <param name="results">
    /// An <see cref="IEnumerable{T}" /> of
    /// <see cref="Result{TOk,TErr}" />
    /// </param>
    /// <typeparam name="TOk">The result's ok value type</typeparam>
    /// <typeparam name="TErr">The result's error value type</typeparam>
    /// <returns>
    /// An <see cref="IEnumerable{T}" /> yielding the error of every
    /// <see cref="Err{TOk,TErr}" /> in the source, in order
    /// </returns>
    public static IEnumerable<TErr> FlattenErr<TOk, TErr>(
        this IEnumerable<Result<TOk, TErr>> results)
        where TOk : notnull where TErr : notnull =>
        results.SelectMany(
            result => result.Match(
                _ => Enumerable.Empty<TErr>(),
                error => new[] { error }.AsEnumerable()));

    /// <summary>
    /// Splits a sequence of results into its ok values and its errors, enumerating
    /// the source exactly once.
    /// </summary>
    /// <remarks>
    /// Deliberately not the all-or-nothing shape: this reports every failure rather
    /// than stopping at the first one.
    /// </remarks>
    /// <param name="results">
    /// An <see cref="IEnumerable{T}" /> of
    /// <see cref="Result{TOk,TErr}" />
    /// </param>
    /// <typeparam name="TOk">The result's ok value type</typeparam>
    /// <typeparam name="TErr">The result's error value type</typeparam>
    /// <returns>
    /// A tuple of the ok values and the errors, each in the order they appeared
    /// in the source
    /// </returns>
    public static (IReadOnlyList<TOk> Oks, IReadOnlyList<TErr> Errs)
        Partition<TOk, TErr>(this IEnumerable<Result<TOk, TErr>> results)
        where TOk : notnull where TErr : notnull
    {
        List<TOk> oks = new List<TOk>();
        List<TErr> errs = new List<TErr>();

        foreach (Result<TOk, TErr> result in results)
        {
            result.Match(oks.Add, errs.Add);
        }

        return (oks, errs);
    }
}
