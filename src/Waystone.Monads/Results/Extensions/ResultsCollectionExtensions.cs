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
    /// Execution is deferred: the source is enumerated as the returned sequence is,
    /// and re-enumerating the result re-enumerates the source. Every error is
    /// dropped rather than stopping the sequence, so a caller who needs the failures
    /// wants <see cref="Partition{TOk,TErr}" /> instead.
    /// </remarks>
    /// <param name="results">
    /// The sequence to read ok values from. Not enumerated until the returned
    /// sequence is.
    /// </param>
    /// <typeparam name="TOk">The result's ok value type</typeparam>
    /// <typeparam name="TErr">The result's error value type</typeparam>
    /// <returns>
    /// The value of every <see cref="Ok{TOk,TErr}" /> in the source, in source
    /// order.
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
    /// Execution is deferred: the source is enumerated as the returned sequence is,
    /// and re-enumerating the result re-enumerates the source.
    /// </remarks>
    /// <param name="results">
    /// The sequence to read errors from. Not enumerated until the returned sequence
    /// is.
    /// </param>
    /// <typeparam name="TOk">The result's ok value type</typeparam>
    /// <typeparam name="TErr">The result's error value type</typeparam>
    /// <returns>
    /// The error of every <see cref="Err{TOk,TErr}" /> in the source, in source
    /// order.
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
    /// Enumerates the source immediately and in full, so it does not stop at the
    /// first <see cref="Err{TOk,TErr}" /> — both lists are complete when the call
    /// returns. Both are empty for an empty source; neither is ever
    /// <see langword="null" />.
    /// </remarks>
    /// <param name="results">The sequence to split. Enumerated once, immediately.</param>
    /// <typeparam name="TOk">The result's ok value type</typeparam>
    /// <typeparam name="TErr">The result's error value type</typeparam>
    /// <returns>
    /// The ok values and the errors as two lists, each in source order.
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
