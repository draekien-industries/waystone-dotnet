namespace Waystone.Monads.Options;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Configs;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>Static functions for <see cref="Option{T}" /></summary>
#if !DEBUG
[DebuggerStepThrough]
#endif
public static class Option
{
    /// <summary>
    /// Tries to store the result of a <paramref name="factory" /> into an
    /// <see cref="Option{T}" />
    /// </summary>
    /// <param name="factory">
    /// A method which when executed will produce the value of
    /// the <see cref="Option{T}" />
    /// </param>
    /// <param name="callerMemberName">The method name of the caller.</param>
    /// <param name="callerLineNumber">The line number of the caller.</param>
    /// <param name="callerArgumentExpression">
    /// The argument expression used as the
    /// factory.
    /// </param>
    /// <typeparam name="T">The factory return value's type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> if the factory executes successfully,
    /// otherwise a <see cref="None{T}" />
    /// </returns>
    public static Option<T> Try<T>(
        Func<T> factory,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(factory))]
        string callerArgumentExpression = "")
        where T : notnull
    {
        try
        {
            T value = factory();
            return Some(value);
        }
        catch (Exception ex)
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadOptions.Global.Log(ex, caller);
            return None<T>();
        }
    }

    /// <summary>
    /// Tries to store the result of an <paramref name="asyncFactory" /> into
    /// an <see cref="Option{T}" />
    /// </summary>
    /// <param name="asyncFactory">
    /// An asynchronous method which when awaited will
    /// produce the value for the <see cref="Option{T}" />
    /// </param>
    /// <param name="callerMemberName">The method name of the caller.</param>
    /// <param name="callerLineNumber">The line number of the caller.</param>
    /// <param name="callerArgumentExpression">
    /// The argument expression used as the
    /// factory.
    /// </param>
    /// <typeparam name="T">The async factory return type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> if the factory succeeds, otherwise a
    /// <see cref="None{T}" />
    /// </returns>
    public static async Task<Option<T>> Try<T>(
        Func<Task<T>> asyncFactory,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(asyncFactory))]
        string callerArgumentExpression = "") where T : notnull
    {
        try
        {
            T value = await asyncFactory();
            return Some(value);
        }
        catch (Exception ex)
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadOptions.Global.Log(ex, caller);
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

    /// <summary>Creates an <see cref="Option{T}" /> from a nullable value type.</summary>
    /// <typeparam name="T">The non-nullable value's type</typeparam>
    /// <param name="value">
    /// The nullable value to convert into an
    /// <see cref="Option{T}" />
    /// </param>
    /// <returns>
    /// Returns a <see cref="Some{T}" /> if the value is not null and not
    /// equal to the default value, otherwise it will return a <see cref="None{T}" />.
    /// </returns>
    public static Option<T> FromNullable<T>(T? value)
        where T : struct =>
        value.HasValue && !value.Value.Equals(default(T))
            ? new Some<T>(value.Value)
            : new None<T>();

    /// <summary>Creates an <see cref="Option{T}" /> from a nullable reference type.</summary>
    /// <typeparam name="T">The non-nullable value's type</typeparam>
    /// <param name="value">
    /// The nullable value to convert into an
    /// <see cref="Option{T}" />
    /// </param>
    /// <returns>
    /// Returns a <see cref="Some{T}" /> if the value is not null, otherwise
    /// returns a <see cref="None{T}" />.
    /// </returns>
    public static Option<T> FromNullable<T>(T? value)
        where T : class =>
        value is null ? new None<T>() : new Some<T>(value);
}
