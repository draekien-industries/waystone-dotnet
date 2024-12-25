namespace Waystone.Monads.Linq;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>LINQ extensions for <see cref="Option{T}" /></summary>
public static class OptionsLinqExtensions
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
}
