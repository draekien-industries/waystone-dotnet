namespace Waystone.Monads.Options.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Extensions for <see cref="Option{T}" /> collections.</summary>
public static class OptionsCollectionExtensions
{
    /// <summary>
    /// Applies a predicate to each option in a sequence, replacing every
    /// <see cref="Some{T}" /> whose value fails the predicate with a
    /// <see cref="None{T}" />.
    /// </summary>
    /// <remarks>
    /// The sequence keeps its length and its order: no element is ever dropped,
    /// so the result holds one option per input option. Follow this with
    /// <see cref="Flatten{T}" /> to get only the values that matched. The
    /// predicate is not called for an element that is already a
    /// <see cref="None{T}" />. Evaluation is deferred — nothing runs until the
    /// result is enumerated, and each enumeration re-runs the predicate.
    /// </remarks>
    /// <param name="options">The sequence to filter.</param>
    /// <param name="predicate">
    /// The condition a <see cref="Some{T}" /> value must satisfy to survive as a
    /// <see cref="Some{T}" />.
    /// </param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// A sequence the same length as <paramref name="options" />, holding each
    /// matching <see cref="Some{T}" /> unchanged and a <see cref="None{T}" /> in
    /// every other position. Empty when <paramref name="options" /> is empty.
    /// </returns>
    public static IEnumerable<Option<T>> Filter<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate) where T : notnull =>
        options.Select(o => o.Filter(predicate));

    /// <summary>
    /// Transforms the value of every <see cref="Some{T}" /> in a sequence,
    /// leaving each <see cref="None{T}" /> in place.
    /// </summary>
    /// <remarks>
    /// The sequence keeps its length and its order. The mapper is not called for
    /// an element that is a <see cref="None{T}" />. Evaluation is deferred —
    /// nothing runs until the result is enumerated, and each enumeration re-runs
    /// the mapper.
    /// </remarks>
    /// <param name="options">The sequence to map.</param>
    /// <param name="mapper">
    /// The transform applied to each <see cref="Some{T}" /> value.
    /// </param>
    /// <typeparam name="TIn">The input option value's type</typeparam>
    /// <typeparam name="TOut">The output option value's type</typeparam>
    /// <returns>
    /// A sequence the same length as <paramref name="options" />, holding the
    /// mapped value in each <see cref="Some{T}" /> position and a
    /// <see cref="None{T}" /> in every other position. Empty when
    /// <paramref name="options" /> is empty.
    /// </returns>
    public static IEnumerable<Option<TOut>> Map<TIn, TOut>(
        this IEnumerable<Option<TIn>> options,
        Func<TIn, TOut> mapper) where TIn : notnull where TOut : notnull =>
        options.Select(o => o.Map(mapper));

    /// <summary>
    /// Returns the values contained by the <see cref="Some{T}" /> elements of a
    /// sequence, skipping the <see cref="None{T}" /> ones.
    /// </summary>
    /// <remarks>
    /// Evaluation is deferred and streams: the source is enumerated once per
    /// enumeration of the result and no intermediate collection is built. This is
    /// the one member here that changes the sequence's length, so its result
    /// cannot be lined up against the source by position.
    /// </remarks>
    /// <param name="options">The sequence to flatten.</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The value of every <see cref="Some{T}" /> in the source, in order. Empty
    /// when <paramref name="options" /> is empty or holds no
    /// <see cref="Some{T}" />.
    /// </returns>
    public static IEnumerable<T> Flatten<T>(
        this IEnumerable<Option<T>> options) where T : notnull =>
        options.SelectMany(option => option.AsEnumerable());

    /// <summary>
    /// Gathers a sequence of options into one option holding every value, or
    /// <see cref="None{T}" /> if any element is absent.
    /// </summary>
    /// <remarks>
    /// This is the all-or-nothing counterpart to <see cref="Flatten{T}" />, which
    /// drops the absent elements instead of failing on them. It is the port of
    /// Rust's <c>collect::&lt;Option&lt;Vec&lt;T&gt;&gt;&gt;()</c> and short-circuits
    /// the same way: enumeration stops at the first <see cref="None{T}" />, so the
    /// tail of <paramref name="options" /> is never visited and a side-effecting
    /// source is left partly consumed. Enumerates when it is called rather than
    /// when its result is read, and builds a list as it goes, so do not call it on
    /// an unbounded sequence.
    /// </remarks>
    /// <param name="options">The sequence to gather. Enumerated immediately.</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> holding one value per element, in source order,
    /// when every element is a <see cref="Some{T}" /> — including when
    /// <paramref name="options" /> is empty, which yields a
    /// <see cref="Some{T}" /> of an empty list rather than a
    /// <see cref="None{T}" />. Otherwise a <see cref="None{T}" />, which carries no
    /// indication of which element was absent.
    /// </returns>
    public static Option<IReadOnlyList<T>> Collect<T>(
        this IEnumerable<Option<T>> options) where T : notnull
    {
        List<T> values = new List<T>();

        foreach (Option<T> option in options)
        {
            bool collected = option.Match(
                values,
                static (value, collecting) =>
                {
                    collecting.Add(value);
                    return true;
                },
                static _ => false);

            if (!collected)
            {
                return Option.None<IReadOnlyList<T>>();
            }
        }

        return Option.Some<IReadOnlyList<T>>(values);
    }

    /// <summary>
    /// Gathers an asynchronous sequence of options into one option holding every
    /// value, or <see cref="None{T}" /> if any element is absent.
    /// </summary>
    /// <remarks>
    /// The asynchronous counterpart of <see cref="Collect{T}" />, and it
    /// short-circuits for real: the stream stops being pulled at the first
    /// <see cref="None{T}" />, so whatever would have produced the later elements
    /// never runs. That is the reason to reach for this over materialising the
    /// stream and calling <see cref="Collect{T}" /> on the result.
    /// </remarks>
    /// <param name="options">
    /// The stream to gather. Pulled from until it ends or an element is absent.
    /// </param>
    /// <param name="cancellationToken">
    /// Passed to the stream's enumerator, so a source that honours it stops
    /// producing when cancellation is requested.
    /// </param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> holding one value per element, in stream order,
    /// when every element is a <see cref="Some{T}" /> — including for an empty
    /// stream. Otherwise a <see cref="None{T}" />.
    /// </returns>
    public static async ValueTask<Option<IReadOnlyList<T>>> CollectAsync<T>(
        this IAsyncEnumerable<Option<T>> options,
        CancellationToken cancellationToken = default) where T : notnull
    {
        List<T> values = new List<T>();

        await foreach (Option<T> option in options
                                          .WithCancellation(cancellationToken)
                                          .ConfigureAwait(false))
        {
            bool collected = option.Match(
                values,
                static (value, collecting) =>
                {
                    collecting.Add(value);
                    return true;
                },
                static _ => false);

            if (!collected)
            {
                return Option.None<IReadOnlyList<T>>();
            }
        }

        return Option.Some<IReadOnlyList<T>>(values);
    }

    /// <summary>
    /// Returns the first <see cref="Some{T}" /> in a sequence whose value
    /// satisfies a predicate.
    /// </summary>
    /// <remarks>
    /// Enumeration stops at the first match, so the tail of
    /// <paramref name="options" /> is never visited. The predicate is not called
    /// for an element that is a <see cref="None{T}" />. Unlike
    /// <see cref="Filter{T}" />, this member enumerates when it is called rather
    /// than when its result is enumerated.
    /// </remarks>
    /// <param name="options">The sequence to search.</param>
    /// <param name="predicate">The condition the matching value must satisfy.</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The first matching <see cref="Some{T}" />, or a <see cref="None{T}" /> when
    /// nothing matches or <paramref name="options" /> is empty. Never
    /// <see langword="null" />.
    /// </returns>
    public static Option<T> FirstOrNone<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate) where T : notnull =>
        options.Filter(predicate).FirstOrDefault(o => o.IsSome)
     ?? Option.None<T>();

    /// <summary>
    /// Returns the value of the first <see cref="Some{T}" /> in a sequence that
    /// satisfies a predicate, or an already-computed fallback.
    /// </summary>
    /// <remarks>
    /// <paramref name="defaultValue" /> is evaluated at the call site whether or not a
    /// match is found; use <see cref="FirstOrElse{T}" /> when producing it is
    /// expensive. Enumeration stops at the first match.
    /// </remarks>
    /// <param name="options">The sequence to search.</param>
    /// <param name="predicate">The condition the matching value must satisfy.</param>
    /// <param name="defaultValue">The value to return when nothing matches.</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The first matching value, or <paramref name="defaultValue" /> when nothing
    /// matches or <paramref name="options" /> is empty.
    /// </returns>
    public static T FirstOr<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate,
        T defaultValue) where T : notnull =>
        options.FirstOrNone(predicate).UnwrapOr(defaultValue);

    /// <summary>
    /// Returns the value of the first <see cref="Some{T}" /> in a sequence that
    /// satisfies a predicate, or a fallback computed on demand.
    /// </summary>
    /// <remarks>
    /// <paramref name="valueFactory" /> runs only when nothing matches, which is the
    /// reason to pick this over <see cref="FirstOr{T}" />. Enumeration stops at
    /// the first match.
    /// </remarks>
    /// <param name="options">The sequence to search.</param>
    /// <param name="predicate">The condition the matching value must satisfy.</param>
    /// <param name="valueFactory">The delegate that produces the value when nothing matches.</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The first matching value, or the value produced by
    /// <paramref name="valueFactory" /> when nothing matches or
    /// <paramref name="options" /> is empty.
    /// </returns>
    public static T FirstOrElse<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate,
        Func<T> valueFactory) where T : notnull =>
        options.FirstOrNone(predicate).UnwrapOrElse(valueFactory);

    /// <summary>
    /// Returns the last <see cref="Some{T}" /> in a sequence whose value
    /// satisfies a predicate.
    /// </summary>
    /// <remarks>
    /// The whole of <paramref name="options" /> is enumerated and the predicate
    /// runs against every <see cref="Some{T}" /> in it, because the last match
    /// cannot be known any earlier. Prefer <see cref="FirstOrNone{T}" /> when
    /// either match will do, and do not call this on an unbounded sequence. The
    /// predicate is not called for an element that is a <see cref="None{T}" />.
    /// </remarks>
    /// <param name="options">The sequence to search.</param>
    /// <param name="predicate">The condition the matching value must satisfy.</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The last matching <see cref="Some{T}" />, or a <see cref="None{T}" /> when
    /// nothing matches or <paramref name="options" /> is empty. Never
    /// <see langword="null" />.
    /// </returns>
    public static Option<T> LastOrNone<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate) where T : notnull =>
        options.Filter(predicate).LastOrDefault(x => x.IsSome)
     ?? Option.None<T>();

    /// <summary>
    /// Returns the value of the last <see cref="Some{T}" /> in a sequence that
    /// satisfies a predicate, or an already-computed fallback.
    /// </summary>
    /// <remarks>
    /// <paramref name="defaultValue" /> is evaluated at the call site whether or not a
    /// match is found; use <see cref="LastOrElse{T}" /> when producing it is
    /// expensive. The whole of <paramref name="options" /> is enumerated, so do
    /// not call this on an unbounded sequence.
    /// </remarks>
    /// <param name="options">The sequence to search.</param>
    /// <param name="predicate">The condition the matching value must satisfy.</param>
    /// <param name="defaultValue">The value to return when nothing matches.</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The last matching value, or <paramref name="defaultValue" /> when nothing
    /// matches or <paramref name="options" /> is empty.
    /// </returns>
    public static T LastOr<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate,
        T defaultValue) where T : notnull =>
        options.LastOrNone(predicate).UnwrapOr(defaultValue);

    /// <summary>
    /// Returns the value of the last <see cref="Some{T}" /> in a sequence that
    /// satisfies a predicate, or a fallback computed on demand.
    /// </summary>
    /// <remarks>
    /// <paramref name="valueFactory" /> runs only when nothing matches, which is the
    /// reason to pick this over <see cref="LastOr{T}" />. The whole of
    /// <paramref name="options" /> is enumerated, so do not call this on an
    /// unbounded sequence.
    /// </remarks>
    /// <param name="options">The sequence to search.</param>
    /// <param name="predicate">The condition the matching value must satisfy.</param>
    /// <param name="valueFactory">The delegate that produces the value when nothing matches.</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The last matching value, or the value produced by
    /// <paramref name="valueFactory" /> when nothing matches or
    /// <paramref name="options" /> is empty.
    /// </returns>
    public static T LastOrElse<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate,
        Func<T> valueFactory) where T : notnull =>
        options.LastOrNone(predicate).UnwrapOrElse(valueFactory);
}
