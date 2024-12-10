namespace Waystone.Monads;

using System;
using Exceptions;

/// <summary>
/// A type that represents either a success (<see cref="Ok{TOk,TErr}" />)
/// or failure (<see cref="Err{TOk,TErr}" />).
/// </summary>
/// <typeparam name="TOk">The type of the <see cref="Ok{TOk,TErr}" /> value.</typeparam>
/// <typeparam name="TErr">The type of the <see cref="Err{TOk,TErr}" /> value.</typeparam>
public interface IResult<TOk, TErr>
    where TOk : notnull
    where TErr : notnull
{
    /// <summary>
    /// Returns <see langword="true" /> if the result is
    /// <see cref="Ok{TOk,TErr}" />.
    /// </summary>
    bool IsOk { get; }

    /// <summary>
    /// Returns <see langword="true" /> if the result is
    /// <see cref="Err{TOk,TErr}" /> .
    /// </summary>
    bool IsErr { get; }


    /// <summary>
    /// Returns <see langword="true" /> if the result is
    /// <see cref="Ok{TOk,TErr}" /> and the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">A <see cref="Predicate{T}" /></param>
    bool IsOkAnd(Predicate<TOk> predicate);

    /// <summary>
    /// Returns <see langword="true" /> if the result is
    /// <see cref="Err{TOk,TErr}" /> and the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">A <see cref="Predicate{T}" />.</param>
    bool IsErrAnd(Predicate<TErr> predicate);

    /// <summary>
    /// Performs a <see langword="switch" /> on the result, invoking the
    /// <paramref name="onOk" /> callback when it is a <see cref="Ok{TOk,TErr}" /> and
    /// the <paramref name="onErr" /> callback when it is a
    /// <see cref="Err{TOk,TErr}" />.
    /// </summary>
    /// <param name="onOk">
    /// A callback for handling the <see cref="Ok{TOk,TErr}" />
    /// case.
    /// </param>
    /// <param name="onErr">
    /// A callback for handling the <see cref="Err{TOk,TErr}" />
    /// case.
    /// </param>
    /// <typeparam name="TOut">The returned type.</typeparam>
    TOut Match<TOut>(Func<TOk, TOut> onOk, Func<TErr, TOut> onErr);

    /// <summary>
    /// Performs a <see langword="switch" /> on the result, invoking the
    /// <paramref name="onOk" /> callback when it is a <see cref="Ok{TOk,TErr}" /> and
    /// the <paramref name="onErr" /> callback when it is a
    /// <see cref="Err{TOk,TErr}" />.
    /// </summary>
    /// <param name="onOk">
    /// A callback for handling the <see cref="Ok{TOk,TErr}" />
    /// case.
    /// </param>
    /// <param name="onErr">
    /// A callback for handling the <see cref="Err{TOk,TErr}" />
    /// case.
    /// </param>
    void Match(Action<TOk> onOk, Action<TErr> onErr);

    /// <summary>
    /// Returns <paramref name="other" /> if the <see langword="this" />
    /// instance is <see cref="Ok{TOk,TErr}" />, otherwise returns the
    /// <see cref="Err{TOk,TErr}" /> value of <see langword="this" /> instance.
    /// </summary>
    /// <example>
    /// <code>
    /// var x = Result.Ok(2);
    /// var y = Result.Err("late error");
    /// Assert.Equal(x.And(y), Result.Err("late error"));
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// var x = Result.Err("early error");
    /// var y = Result.Ok(2);
    /// Assert.Equal(x.And(y), Result.Err("early error"));
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// var x = Result.Err("first error");
    /// var y = Result.Err("second error");
    /// Assert.Equal(x.And(y), Result.Err("first error"));
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// var x = Result.Ok(2);
    /// var y = Result.Ok("different result type");
    /// Assert.Equal(x.And(y), Result.Ok("different result type"));
    /// </code>
    /// </example>
    /// <param name="other">The other result type.</param>
    /// <typeparam name="TOk2">
    /// The <see cref="Ok{TOk,TErr}" /> value's type of the
    /// other result.
    /// </typeparam>
    IResult<TOk2, TErr> And<TOk2>(IResult<TOk2, TErr> other)
        where TOk2 : notnull;

    /// <summary>
    /// Calls the <paramref name="createOther" /> if the result is
    /// <see cref="Ok{TOk,TErr}" />, otherwise returns the <see cref="Err{TOk,TErr}" />
    /// value of <see langword="this" /> instance.
    /// </summary>
    /// <param name="createOther">A function that creates the other result.</param>
    /// <typeparam name="TOk2">
    /// The <see cref="Ok{TOk,TErr}" /> value's type of the
    /// other result.
    /// </typeparam>
    IResult<TOk2, TErr> AndThen<TOk2>(
        Func<TOk, IResult<TOk2, TErr>> createOther) where TOk2 : notnull;

    /// <summary>
    /// Returns <paramref name="other" /> if the result is
    /// <see cref="Err{TOk,TErr}" />, otherwise returns the <see cref="Ok{TOk,TErr}" />
    /// value of this result instance.
    /// </summary>
    /// <param name="other">The other result.</param>
    /// <typeparam name="TErr2">The other result's error value type</typeparam>
    IResult<TOk, TErr2> Or<TErr2>(IResult<TOk, TErr2> other)
        where TErr2 : notnull;

    /// <summary>
    /// Calls <paramref name="createOther" /> if the result is
    /// <see cref="Err{TOk,TErr}" />, otherwise returns the <see cref="Ok{TOk,TErr}" />
    /// value of this result instance.
    /// </summary>
    /// <remarks>This function can be used for control flow based on result values.</remarks>
    /// <param name="createOther">A function which creates the other result.</param>
    /// <typeparam name="TErr2">The other result's error value type.</typeparam>
    IResult<TOk, TErr2> OrElse<TErr2>(
        Func<TErr, IResult<TOk, TErr2>> createOther) where TErr2 : notnull;

    /// <summary>
    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value, consuming the
    /// result instance.
    /// </summary>
    /// <remarks>
    /// Because this function may throw an
    /// <see cref="UnmetExpectationException" />, its use is generally discouraged.
    /// Instead, prefer to use the <see cref="Match{TOut}" /> function and handling the
    /// <see cref="Err{TOk,TErr}" /> case explicitly, or call <see cref="UnwrapOr" />,
    /// <see cref="UnwrapOrElse" />, or <see cref="UnwrapOrDefault" />.
    /// </remarks>
    /// <exception cref="UnmetExpectationException">
    /// Throws if the value is an
    /// <see cref="Err{TOk,TErr}" />, with an exception message including the passed
    /// <paramref name="message" />, and the content of the
    /// <see cref="Err{TOk,TErr}" />
    /// </exception>
    /// <param name="message">The custom exception message.</param>
    TOk Expect(string message);

    /// <summary>
    /// Returns the contained <see cref="Err{TOk,TErr}" /> value, consuming
    /// the result instance.
    /// </summary>
    /// <param name="message">The custom exception message.</param>
    /// <exception cref="UnmetExpectationException">
    /// Throws if the value is an
    /// <see cref="Ok{TOk,TErr}" />, with a message including the passed
    /// <paramref name="message" />, and the content of the <see cref="Ok{TOk,TErr}" />
    /// </exception>
    TErr ExpectErr(string message);

    /// <summary>
    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value, consuming the
    /// result instance.
    /// </summary>
    /// <remarks>
    /// Because this function may throw an <see cref="UnwrapException" />, its
    /// use is generally discouraged. Instead, prefer to use the
    /// <see cref="Match{TOut}" /> function and handling the
    /// <see cref="Err{TOk,TErr}" /> case explicitly, or call <see cref="UnwrapOr" />,
    /// <see cref="UnwrapOrElse" />, or <see cref="UnwrapOrDefault" />.
    /// </remarks>
    /// <exception cref="UnwrapException">
    /// Throws if the value is an
    /// <see cref="Err{TOk,TErr}" />, with an exception message provided by the
    /// <see cref="Err{TOk,TErr}" /> value.
    /// </exception>
    TOk Unwrap();

    /// <summary>
    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value or a provided
    /// default.
    /// </summary>
    /// <param name="default">
    /// The default value to return on an
    /// <see cref="Err{TOk,TErr}" />
    /// </param>
    TOk UnwrapOr(TOk @default);

    /// <summary>
    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value or the default
    /// value for <typeparamref name="TOk" />
    /// </summary>
    TOk? UnwrapOrDefault();

    /// <summary>
    /// Returns the contained <see cref="Ok{TOk,TErr}" /> value or computes it
    /// from the callback function.
    /// </summary>
    /// <param name="onErr">
    /// The callback function for computing the
    /// <see cref="Err{TOk,TErr}" /> return value.
    /// </param>
    TOk UnwrapOrElse(Func<TErr, TOk> onErr);

    /// <summary>
    /// Returns the contained <see cref="Err{TOk,TErr}" /> value, consuming
    /// the result instance.
    /// </summary>
    /// <exception cref="UnwrapException">
    /// Throws if the value is an
    /// <see cref="Err{TOk,TErr}" />, with a custom exception message provided by the
    /// <see cref="Ok{TOk,TErr}" />'s value.
    /// </exception>
    TErr UnwrapErr();

    /// <summary>
    /// Calls a function with a reference to the contained value if
    /// <see cref="Ok{TOk,TErr}" />
    /// </summary>
    /// <param name="action">The function to be invoked.</param>
    IResult<TOk, TErr> Inspect(Action<TOk> action);

    /// <summary>
    /// Calls a function with a reference to the contained value if
    /// <see cref="Err{TOk,TErr}" />
    /// </summary>
    /// <param name="action">The function to be invoked.</param>
    IResult<TOk, TErr> InspectErr(Action<TErr> action);

    /// <summary>
    /// Maps a <c>Result&lt;TOk, TErr&gt;</c> to
    /// <c>Result&lt;TOk2, TErr&gt;</c> by applying a function to a contained
    /// <see cref="Ok{TOk,TErr}" /> value, leaving an <see cref="Err{TOk,TErr}" />
    /// untouched.
    /// </summary>
    /// <remarks>This function can be used to compose the results of two functions.</remarks>
    /// <param name="map">The map function.</param>
    /// <typeparam name="TOk2">The output value type.</typeparam>
    IResult<TOk2, TErr> Map<TOk2>(Func<TOk, TOk2> map) where TOk2 : notnull;

    /// <summary>
    /// Returns the provided default (if <see cref="Err{TOk,TErr}" />), or
    /// applies a function to the contained value (if <see cref="Ok{TOk,TErr}" />).
    /// </summary>
    /// <param name="default">
    /// The default value for an <see cref="Err{TOk,TErr}" />
    /// </param>
    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
    /// <typeparam name="TOk2">The mapped result value type</typeparam>
    TOk2 MapOr<TOk2>(TOk2 @default, Func<TOk, TOk2> map);

    /// <summary>
    /// Maps a <c>Result&lt;TOk, TErr&gt;</c> to <typeparamref name="TOk2" />
    /// by applying fallback function <paramref name="createDefault" /> to a contained
    /// <see cref="Err{TOk,TErr}" /> value, or function <see cref="Map{TOk2}" /> to a
    /// contained <see cref="Ok{TOk,TErr}" /> value.
    /// </summary>
    /// <param name="createDefault">
    /// A function to create the default value for an
    /// <see cref="Err{TOk,TErr}" />
    /// </param>
    /// <param name="map">The map function for an <see cref="Ok{TOk,TErr}" /></param>
    /// <typeparam name="TOk2">The mapped result value type</typeparam>
    /// <returns></returns>
    TOk2 MapOrElse<TOk2>(
        Func<TErr, TOk2> createDefault,
        Func<TOk, TOk2> map);

    /// <summary>
    /// Maps a <c>Result&lt;TOk, TErr&gt;</c> to
    /// <c>Result&lt;TOk, TErr2&gt;</c> by applying a function to a contained
    /// <see cref="Err{TOk,TErr}" /> value, leaving an <see cref="Ok{TOk,TErr}" />
    /// value untouched.
    /// </summary>
    /// <remarks>
    /// This function can be used to pass through a successful result while
    /// handling an error.
    /// </remarks>
    /// <param name="map">
    /// The map function to apply to the <see cref="Err{TOk,TErr}" />
    /// </param>
    /// <typeparam name="TErr2">The output error value type</typeparam>
    IResult<TOk, TErr2> MapErr<TErr2>(Func<TErr, TErr2> map)
        where TErr2 : notnull;

    /// <summary>
    /// Converts from a <see cref="IResult{TOk,TErr}" /> into an
    /// <c>IOption&lt;TOk&gt;</c>
    /// </summary>
    /// <remarks>
    /// Converts the result instance into an <see cref="IOption{T}" />,
    /// consuming the result instance, and discarding the error, if any.
    /// </remarks>
    IOption<TOk> ToOk();

    /// <summary>
    /// Converts from a <see cref="IResult{TOk,TErr}" /> to
    /// <c>IOption&lt;TErr&gt;</c>
    /// </summary>
    /// <remarks>
    /// Converts this result instance into an <see cref="IOption{T}" />,
    /// consuming the result instance, and discarding the success value, if any.
    /// </remarks>
    IOption<TErr> ToErr();
}