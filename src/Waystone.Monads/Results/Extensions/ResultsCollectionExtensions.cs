namespace Waystone.Monads.Results.Extensions;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Options;

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
    /// Gathers a sequence of results into one result holding every ok value, or the
    /// first error encountered.
    /// </summary>
    /// <remarks>
    /// The all-or-nothing counterpart to <see cref="Partition{TOk,TErr}" />, which
    /// reports every failure and always succeeds. This is the port of Rust's
    /// <c>collect::&lt;Result&lt;Vec&lt;T&gt;, E&gt;&gt;()</c> and short-circuits the
    /// same way: enumeration stops at the first <see cref="Err{TOk,TErr}" />, so
    /// later elements are never visited, later errors are never seen, and a
    /// side-effecting source is left partly consumed. Reach for
    /// <see cref="Partition{TOk,TErr}" /> when the caller needs to report all of the
    /// failures rather than fail on one. Enumerates when it is called rather than
    /// when its result is read, so do not call it on an unbounded sequence.
    /// </remarks>
    /// <param name="results">The sequence to gather. Enumerated immediately.</param>
    /// <typeparam name="TOk">The result's ok value type</typeparam>
    /// <typeparam name="TErr">The result's error value type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> holding one value per element, in source
    /// order, when every element is an <see cref="Ok{TOk,TErr}" /> — including when
    /// <paramref name="results" /> is empty, which succeeds with an empty list.
    /// Otherwise an <see cref="Err{TOk,TErr}" /> carrying the first error, with the
    /// ok values that preceded it discarded.
    /// </returns>
    public static Result<IReadOnlyList<TOk>, TErr> Collect<TOk, TErr>(
        this IEnumerable<Result<TOk, TErr>> results)
        where TOk : notnull where TErr : notnull
    {
        List<TOk> oks = new List<TOk>();
        Option<TErr> failure = Option.None<TErr>();

        foreach (Result<TOk, TErr> result in results)
        {
            failure = result.Match(
                oks,
                static (ok, collecting) =>
                {
                    collecting.Add(ok);
                    return Option.None<TErr>();
                },
                static (error, _) => Option.Some(error));

            if (failure.IsSome)
            {
                break;
            }
        }

        return failure.Match(
            oks,
            static (error, _) => Result.Err<IReadOnlyList<TOk>, TErr>(error),
            static collected => Result.Ok<IReadOnlyList<TOk>, TErr>(collected));
    }

    /// <summary>
    /// Gathers an asynchronous sequence of results into one result holding every ok
    /// value, or the first error encountered.
    /// </summary>
    /// <remarks>
    /// The asynchronous counterpart of <see cref="Collect{TOk,TErr}" />, and it
    /// short-circuits for real: the stream stops being pulled at the first
    /// <see cref="Err{TOk,TErr}" />, so whatever would have produced the later
    /// elements never runs. That is the reason to reach for this over materialising
    /// the stream and calling <see cref="Collect{TOk,TErr}" /> on the result.
    /// </remarks>
    /// <param name="results">
    /// The stream to gather. Pulled from until it ends or an element fails.
    /// </param>
    /// <param name="cancellationToken">
    /// Passed to the stream's enumerator, so a source that honours it stops
    /// producing when cancellation is requested.
    /// </param>
    /// <typeparam name="TOk">The result's ok value type</typeparam>
    /// <typeparam name="TErr">The result's error value type</typeparam>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> holding one value per element, in stream
    /// order, when every element is an <see cref="Ok{TOk,TErr}" /> — including for
    /// an empty stream. Otherwise an <see cref="Err{TOk,TErr}" /> carrying the first
    /// error.
    /// </returns>
    public static async ValueTask<Result<IReadOnlyList<TOk>, TErr>>
        CollectAsync<TOk, TErr>(
            this IAsyncEnumerable<Result<TOk, TErr>> results,
            CancellationToken cancellationToken = default)
        where TOk : notnull where TErr : notnull
    {
        List<TOk> oks = new List<TOk>();
        Option<TErr> failure = Option.None<TErr>();

        await foreach (Result<TOk, TErr> result in results
                                                  .WithCancellation(
                                                       cancellationToken)
                                                  .ConfigureAwait(false))
        {
            failure = result.Match(
                oks,
                static (ok, collecting) =>
                {
                    collecting.Add(ok);
                    return Option.None<TErr>();
                },
                static (error, _) => Option.Some(error));

            if (failure.IsSome)
            {
                break;
            }
        }

        return failure.Match(
            oks,
            static (error, _) => Result.Err<IReadOnlyList<TOk>, TErr>(error),
            static collected => Result.Ok<IReadOnlyList<TOk>, TErr>(collected));
    }

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
