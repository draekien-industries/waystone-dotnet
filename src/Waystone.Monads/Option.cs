namespace Waystone.Monads;

using System;
using System.Threading.Tasks;
using Exceptions;

/// <summary>Static functions for <see cref="Option{T}" /></summary>
public static class Option
{
    /// <summary>
    /// Binds the result of a <paramref name="factory" /> into an
    /// <see cref="Option{T}" />
    /// </summary>
    /// <param name="factory">
    /// A method which when executed will produce the value of
    /// the <see cref="Option{T}" />
    /// </param>
    /// <param name="onError">
    /// Optional. Provides access to any exceptions the factory
    /// throws. Not providing a callback will mean the exception gets swallowed.
    /// </param>
    /// <typeparam name="T">The factory return value's type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> if the factory executes successfully,
    /// otherwise a <see cref="None{T}" />
    /// </returns>
    public static Option<T> Bind<T>(
        Func<T> factory,
        Action<Exception>? onError = null)
        where T : notnull
    {
        try
        {
            T value = factory();
            return Some(value);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            return None<T>();
        }
    }

    /// <summary>
    /// Binds the result of an <paramref name="asyncFactory" /> into an
    /// <see cref="Option{T}" />
    /// </summary>
    /// <param name="asyncFactory">
    /// An asynchronous method which when awaited will
    /// produce the value for the <see cref="Option{T}" />
    /// </param>
    /// <param name="onError">
    /// Optional. Provides access to any exceptions the factory
    /// throws. Not providing a callback will mean the exception gets swallowed.
    /// </param>
    /// <typeparam name="T">The async factory return type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> if the factory succeeds, otherwise a
    /// <see cref="None{T}" />
    /// </returns>
    public static async Task<Option<T>> BindAsync<T>(
        Func<Task<T>> asyncFactory,
        Action<Exception>? onError = null) where T : notnull
    {
        try
        {
            T value = await asyncFactory();
            return Some(value);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            return None<T>();
        }
    }

    /// <summary>Creates a <see cref="Some{T}" /></summary>
    /// <param name="value">The value of the <see cref="Some{T}" /></param>
    /// <typeparam name="T">The option value's type.</typeparam>
    /// <returns>An <see cref="Option{T}" />.</returns>
    public static Option<T> Some<T>(T value) where T : notnull =>
        new Some<T>(value);

    /// <summary>Creates a <see cref="None{T}" /></summary>
    /// <typeparam name="T">The option value's type.</typeparam>
    /// <returns>An <see cref="Option{T}" />.</returns>
    public static Option<T> None<T>() where T : notnull => new None<T>();

    /// <summary>
    /// Converts an <see cref="Option{T}" /> of a <see cref="Task{T}" /> into
    /// a <see cref="Task{T}" /> of an <see cref="Option{T}" />
    /// </summary>
    /// <param name="optionOfTask">An option of a task</param>
    /// <param name="onError">
    /// Optional. A callback which will be invoked if the
    /// resolution of the <see cref="Task{T}" /> throws an exception. Not providing one
    /// will mean the exception gets swallowed.
    /// </param>
    /// <typeparam name="T">The return value of the task</typeparam>
    /// <returns>A task of an option</returns>
    public static async Task<Option<T>> Awaited<T>(
        this Option<Task<T>> optionOfTask,
        Action<Exception>? onError = null)
        where T : notnull
    {
        return await optionOfTask.Match(
            async value =>
            {
                try
                {
                    return Some(await value);
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                    return None<T>();
                }
            },
            () => Task.FromResult(None<T>()));
    }

    /// <summary>Unzips an option containing a tuple value into two options.</summary>
    /// <param name="option">The option to be unzipped.</param>
    /// <typeparam name="T1">The first option value's type.</typeparam>
    /// <typeparam name="T2">The second option value's type.</typeparam>
    /// <returns>
    /// If <paramref name="option" /> is <c>Some&lt;(T1, T2)&gt;</c> this
    /// method returns <c>(Some&lt;T1&gt;, Some&lt;T2&gt;)</c>. Otherwise
    /// <c>(None&lt;T1&gt;, None&lt;T2&gt;)</c> is returned.
    /// </returns>
    public static (Option<T1>, Option<T2>) Unzip<T1, T2>(
        this Option<(T1, T2)> option) where T1 : notnull where T2 : notnull =>
        option.Match(
            tuple => (Some(tuple.Item1), Some(tuple.Item2)),
            () => (None<T1>(), None<T2>()));

    /// <summary>
    /// Converts from <c>Option&lt;Option&lt;T&gt;&gt;</c> to
    /// <c>Option&lt;T&gt;</c>.
    /// </summary>
    /// <remarks>Flattening only removes one level of nesting at a time.</remarks>
    /// <param name="option">The option that needs to be flattened.</param>
    /// <typeparam name="T">The option value's type.</typeparam>
    public static Option<T> Flatten<T>(this Option<Option<T>> option)
        where T : notnull =>
        option.Match(innerOption => innerOption, None<T>);

    /// <summary>
    /// Transposes an <see cref="Option{T}" /> of a
    /// <see cref="Result{TOk,TErr}" /> into a <see cref="Result{TOk,TErr}" /> of an
    /// <see cref="Option{T}" />.
    /// </summary>
    /// <list type="bullet">
    /// <item>
    /// <see cref="None{T}" /> will be mapped to <see cref="Ok{TOk,TErr}" /> of
    /// <see cref="None{T}" />
    /// </item>
    /// <item>
    /// <see cref="Some{T}" /> of <see cref="Ok{TOk,TErr}" /> and
    /// <see cref="Some{T}" /> of <see cref="None{T}" /> will be mapped to
    /// <see cref="Ok{TOk,TErr}" /> of <see cref="Some{T}" /> and
    /// <see cref="Err{TOk,TErr}" />
    /// </item>
    /// </list>
    /// <param name="option">The option to transpose into a result</param>
    /// <typeparam name="TOk">The ok result value type</typeparam>
    /// <typeparam name="TErr">The error result value type</typeparam>
    public static Result<Option<TOk>, TErr> Transpose<TOk, TErr>(
        this Option<Result<TOk, TErr>> option)
        where TOk : notnull where TErr : notnull =>
        option.Match(
            some => some.Match(
                ok => Result.Ok<Option<TOk>, TErr>(Some(ok)),
                Result.Err<Option<TOk>, TErr>),
            () => Result.Ok<Option<TOk>, TErr>(None<TOk>()));
}

/// <summary>
/// A type which can be in two states, a <see cref="Some{T}" /> or a
/// <see cref="None{T}" />.
/// </summary>
/// <typeparam name="T">The option value's type.</typeparam>
public abstract record Option<T> where T : notnull
{
    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="Some{T}" /> value.
    /// </summary>
    public abstract bool IsSome { get; }

    /// <summary>
    /// Returns <see langword="false" /> if the option is a
    /// <see cref="None{T}" /> value.
    /// </summary>
    public abstract bool IsNone { get; }

    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="Some{T}" /> and the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">A <see cref="Predicate{T}" /></param>
    public abstract bool IsSomeAnd(Predicate<T> predicate);

    /// <summary>
    /// Returns <see langword="true" /> if the option is a
    /// <see cref="None{T}" /> or the value inside of it matches a predicate.
    /// </summary>
    /// <param name="predicate">A <see cref="Predicate{T}" /></param>
    public abstract bool IsNoneOr(Predicate<T> predicate);

    /// <summary>
    /// Performs a <see langword="switch" /> on the option, invoking the
    /// <paramref name="onSome" /> callback when it is a <see cref="Some{T}" /> and the
    /// <paramref name="onNone" /> callback when it is a  <see cref="None{T}" />.
    /// </summary>
    /// <param name="onSome">
    /// A <see cref="Func{T, TResult}" /> for handling the
    /// <see cref="Some{T}" /> case.
    /// </param>
    /// <param name="onNone">
    /// A <see cref="Func{TResult}" /> for handling the
    /// <see cref="None{T}" /> case.
    /// </param>
    /// <typeparam name="TOut">The returned type.</typeparam>
    /// <returns>
    /// The output of either the <paramref name="onSome" /> or
    /// <paramref name="onNone" /> callback.
    /// </returns>
    public abstract TOut Match<TOut>(Func<T, TOut> onSome, Func<TOut> onNone);

    /// <summary>
    /// Performs a <see langword="switch" /> on the option, invoking the
    /// <paramref name="onSome" /> callback when it is a <see cref="Some{T}" /> and the
    /// <paramref name="onNone" /> callback when it is a  <see cref="None{T}" />.
    /// </summary>
    /// <param name="onSome">
    /// A <see cref="Action{T}" /> for handling the
    /// <see cref="Some{T}" /> case.
    /// </param>
    /// <param name="onNone">
    /// A <see cref="Action" /> for handling the
    /// <see cref="None{T}" /> case.
    /// </param>
    public abstract void Match(Action<T> onSome, Action onNone);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value, consuming the
    /// <see cref="Option{T}" />.
    /// </summary>
    /// <param name="message">A custom exception message</param>
    /// <exception cref="UnmetExpectationException">
    /// Thrown if the value is a
    /// <see cref="None{T}" /> with a custom message provided by
    /// <paramref name="message" />
    /// </exception>
    public abstract T Expect(string message);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value, consuming the
    /// <see cref="Option{T}" />.
    /// </summary>
    /// <remarks>
    /// Because this function may throw an exception, its use is generally
    /// discouraged. Instead, prefer to use the <see cref="Match{TOut}" /> function and
    /// handle the <see cref="None{T}" /> case explicitly, or call
    /// <see cref="UnwrapOr" />, <see cref="UnwrapOrElse" />, or
    /// <see cref="UnwrapOrDefault" />.
    /// </remarks>
    /// <exception cref="UnwrapException">
    /// Throws if the option equals
    /// <see cref="None{T}" />
    /// </exception>
    public abstract T Unwrap();

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value or a provided
    /// default.
    /// </summary>
    /// <param name="value">
    /// The default value to return on a <see cref="None{T}" />
    /// </param>
    public abstract T UnwrapOr(T value);

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value or the
    /// <see langword="default" /> of <typeparamref name="T" />.
    /// </summary>
    public abstract T? UnwrapOrDefault();

    /// <summary>
    /// Returns the contained <see cref="Some{T}" /> value or computes it from
    /// a delegate.
    /// </summary>
    /// <param name="else">
    /// The delegate which computes the <see cref="None{T}" />
    /// value.
    /// </param>
    public abstract T UnwrapOrElse(Func<T> @else);

    /// <summary>
    /// Maps an <c>Option&lt;T&gt;</c> to an <c>Option&lt;T2&gt;</c> by
    /// applying a function to a contained value (if <see cref="Some{T}" />) or returns
    /// <see cref="None{T}" /> (if <see cref="None{T}" />).
    /// </summary>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    public abstract Option<T2> Map<T2>(Func<T, T2> map) where T2 : notnull;

    /// <summary>
    /// Returns the provided default result (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <param name="default">The default value for a <see cref="None{T}" />.</param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    public abstract T2 MapOr<T2>(T2 @default, Func<T, T2> map);

    /// <summary>
    /// Computes a default from a function (if <see cref="None{T}" />), or
    /// applies a function to the contained value (if <see cref="Some{T}" />).
    /// </summary>
    /// <param name="createDefault">
    /// The function that will create a default value for a
    /// <see cref="None{T}" />.
    /// </param>
    /// <param name="map">The map function.</param>
    /// <typeparam name="T2">The return type of the map function.</typeparam>
    public abstract T2 MapOrElse<T2>(Func<T2> createDefault, Func<T, T2> map);

    /// <summary>
    /// Calls a function with a reference to the contained value if
    /// <see cref="Some{T}" />
    /// </summary>
    /// <param name="action">The function to execute against the value.</param>
    /// <returns>The original <see cref="Option{T}" /></returns>
    public abstract Option<T> Inspect(Action<T> action);

    /// <summary>
    /// Returns <see cref="None{T}" /> if the option is <see cref="None{T}" />,
    /// otherwise calls the <paramref name="predicate" /> with the wrapped value and
    /// returns:
    /// <list type="bullet">
    /// <item>
    /// <see cref="Some{T}" /> if the <paramref name="predicate" /> returns
    /// <see langword="true" /> (where <typeparamref name="T" /> is the wrapped value),
    /// and
    /// </item>
    /// <item>
    /// <see cref="None{T}" /> if the <paramref name="predicate" /> returns
    /// <see langword="false" />.
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="predicate">The filter function.</param>
    public abstract Option<T> Filter(Predicate<T> predicate);

    /// <summary>
    /// Returns the option if it contains a value, otherwise returns
    /// <paramref name="other" />
    /// </summary>
    /// <param name="other">The other option.</param>
    public abstract Option<T> Or(Option<T> other);

    /// <summary>
    /// Returns the option if it contains a value, otherwise invokes the
    /// <paramref name="createElse" /> function and returns the result.
    /// </summary>
    /// <param name="createElse">The function that will create the other option.</param>
    public abstract Option<T> OrElse(Func<Option<T>> createElse);

    /// <summary>
    /// Returns <see cref="Some{T}" /> if exactly one of
    /// <see langword="this" /> or <paramref name="other" /> is <see cref="Some{T}" />,
    /// otherwise returns <see cref="None{T}" />.
    /// </summary>
    /// <param name="other">The other option.</param>
    public abstract Option<T> Xor(Option<T> other);

    /// <summary>
    /// Zips the current option with another option, combining the values into
    /// a tuple.
    /// </summary>
    /// <param name="other">The other option.</param>
    /// <typeparam name="T2">The type of the value contained in the other option.</typeparam>
    /// <returns>
    /// If the current option is <see cref="Some{T}" /> and
    /// <paramref name="other" /> is <see cref="Some{T}" />, this method returns
    /// <c>Some&lt;(T, T2)&gt;</c>. Otherwise, <c>None&lt;(T, T2)&gt;</c> is returned.
    /// </returns>
    public abstract Option<(T, T2)> Zip<T2>(Option<T2> other)
        where T2 : notnull;

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T" /> into an
    /// <see cref="Option{T}" />
    /// </summary>
    /// <param name="value">The value of the option</param>
    /// <returns>
    /// A <see cref="Some{T}" /> when the value is not the default of its
    /// type, otherwise a <see cref="None{T}" />
    /// </returns>
    public static implicit operator Option<T>(T value) =>
        Equals(value, default(T)) ? new None<T>() : new Some<T>(value);
}
