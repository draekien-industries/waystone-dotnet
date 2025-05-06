namespace Waystone.Monads.Options.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Extensions for <see cref="Option{T}" /> collections.</summary>
public static class OptionsCollectionExtensions
{
    /// <summary>
    /// Filters a sequence of options based on a predicate, converting options
    /// that fail to match the criteria into <see cref="None{T}" />.
    /// </summary>
    /// <param name="options">
    /// An <see cref="IEnumerable{T}" /> of
    /// <see cref="Option{T}" />
    /// </param>
    /// <param name="predicate">
    /// A function to test each <see cref="Option{T}" /> for a
    /// condition
    /// </param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// An <see cref="IEnumerable{T}" /> of <see cref="Option{T}" /> that
    /// contain elements that are <see cref="Some{T}" /> when they match the predicate
    /// and  are <see cref="None{T}" /> when they failed to match the predicate
    /// </returns>
    public static IEnumerable<Option<T>> Filter<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate) where T : notnull =>
        options.Select(o => o.Filter(predicate));

    /// <summary>
    /// Applies a map function onto a sequence of options, transforming the
    /// values of the <see cref="Some{T}" /> options
    /// </summary>
    /// <param name="options">
    /// An <see cref="IEnumerable{T}" /> of
    /// <see cref="Option{T}" />
    /// </param>
    /// <param name="mapper">
    /// A map function that converts from
    /// <typeparamref name="TIn" /> to <typeparamref name="TOut" />
    /// </param>
    /// <typeparam name="TIn">The input option value's type</typeparam>
    /// <typeparam name="TOut">The output option value's type</typeparam>
    /// <returns>
    /// An <see cref="IEnumerable{T}" /> of <see cref="Option{T}" /> that
    /// contain elements transformed by the mapper
    /// </returns>
    public static IEnumerable<Option<TOut>> Map<TIn, TOut>(
        this IEnumerable<Option<TIn>> options,
        Func<TIn, TOut> mapper) where TIn : notnull where TOut : notnull =>
        options.Select(o => o.Map(mapper));

    /// <summary>
    /// Returns the first <see cref="Option{T}" /> of the sequence that
    /// satisfies a condition or a <see cref="None{T}" /> if a match is not found
    /// </summary>
    /// <param name="options">
    /// An <see cref="IEnumerable{T}" /> of
    /// <see cref="Option{T}" />
    /// </param>
    /// <param name="predicate">
    /// A function to test each <see cref="Option{T}" /> for a
    /// condition
    /// </param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The first <see cref="Option{T}" /> that passed the condition specified
    /// by the predicate, otherwise a <see cref="None{T}" />
    /// </returns>
    public static Option<T> FirstOrNone<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate) where T : notnull =>
        options.Filter(predicate).FirstOrDefault(o => o.IsSome)
     ?? Option.None<T>();

    /// <summary>
    /// Returns the first element of the sequence that satisfies a condition,
    /// or a default value if a match is not found
    /// </summary>
    /// <param name="options">
    /// An <see cref="IEnumerable{T}" /> of
    /// <see cref="Option{T}" />
    /// </param>
    /// <param name="predicate">
    /// A function to test each <see cref="Option{T}" /> for a
    /// condition
    /// </param>
    /// <param name="default">The default value to return when a match is not found</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The first value that passed the condition specified by the predicate,
    /// otherwise the provided default
    /// </returns>
    public static T FirstOr<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate,
        T @default) where T : notnull =>
        options.FirstOrNone(predicate).UnwrapOr(@default);

    /// <summary>
    /// Returns the first element of the sequence that satisfies a condition,
    /// or the default value created from a delegate if a match is not found
    /// </summary>
    /// <param name="options">
    /// An <see cref="IEnumerable{T}" /> of
    /// <see cref="Option{T}" />
    /// </param>
    /// <param name="predicate">
    /// A function to test each <see cref="Option{T}" /> for a
    /// condition
    /// </param>
    /// <param name="else">The delegate that will create the else value</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The first value that passed the condition specified by the predicate,
    /// otherwise the default created by the else delegate
    /// </returns>
    public static T FirstOrElse<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate,
        Func<T> @else) where T : notnull =>
        options.FirstOrNone(predicate).UnwrapOrElse(@else);

    /// <summary>
    /// Returns the last <see cref="Option{T}" /> of the sequence that
    /// satisfies a condition or a <see cref="None{T}" /> if a match is not found
    /// </summary>
    /// <param name="options">
    /// An <see cref="IEnumerable{T}" /> of
    /// <see cref="Option{T}" />
    /// </param>
    /// <param name="predicate">
    /// A function to test each <see cref="Option{T}" /> for a
    /// condition
    /// </param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The last <see cref="Option{T}" /> that passed the condition specified
    /// by the predicate, otherwise a <see cref="None{T}" />
    /// </returns>
    public static Option<T> LastOrNone<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate) where T : notnull =>
        options.Filter(predicate).LastOrDefault(x => x.IsSome)
     ?? Option.None<T>();

    /// <summary>
    /// Returns the last element of the sequence that satisfies a condition,
    /// or a default value if a match is not found
    /// </summary>
    /// <param name="options">
    /// An <see cref="IEnumerable{T}" /> of
    /// <see cref="Option{T}" />
    /// </param>
    /// <param name="predicate">
    /// A function to test each <see cref="Option{T}" /> for a
    /// condition
    /// </param>
    /// <param name="default">The default value to return when a match is not found</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The last value that passed the condition specified by the predicate,
    /// otherwise the provided default
    /// </returns>
    public static T LastOr<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate,
        T @default) where T : notnull =>
        options.LastOrNone(predicate).UnwrapOr(@default);

    /// <summary>
    /// Returns the last element of the sequence that satisfies a condition,
    /// or the default value created from a delegate if a match is not found
    /// </summary>
    /// <param name="options">
    /// An <see cref="IEnumerable{T}" /> of
    /// <see cref="Option{T}" />
    /// </param>
    /// <param name="predicate">
    /// A function to test each <see cref="Option{T}" /> for a
    /// condition
    /// </param>
    /// <param name="else">The delegate that will create the else value</param>
    /// <typeparam name="T">The option value's type</typeparam>
    /// <returns>
    /// The last value that passed the condition specified by the predicate,
    /// otherwise the default created by the else delegate
    /// </returns>
    public static T LastOrElse<T>(
        this IEnumerable<Option<T>> options,
        Func<T, bool> predicate,
        Func<T> @else) where T : notnull =>
        options.LastOrNone(predicate).UnwrapOrElse(@else);
}
