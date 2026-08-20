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
    /// A <see cref="Some{T}" /> if the factory produces a value that a
    /// <see cref="Some{T}" /> can hold, otherwise a <see cref="None{T}" />.
    /// </returns>
    /// <remarks>
    /// A <see cref="None{T}" /> is returned both when the factory throws and
    /// when it returns a value a <see cref="Some{T}" /> cannot hold. Only the
    /// thrown case is reported to the exception logger configured on
    /// <see cref="MonadOptions" />, because the implicit conversion to
    /// <see cref="Option{T}" /> decides which values a <see cref="Some{T}" />
    /// can hold and returns <see cref="None{T}" /> rather than throwing. That
    /// conversion is applied inside the try, so the two cannot disagree and
    /// nothing it throws escapes.
    /// </remarks>
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
            return factory();
        }
        catch (Exception ex)
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadOptions.Current.Log(ex, caller);
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
    /// A <see cref="Some{T}" /> if the factory produces a value that a
    /// <see cref="Some{T}" /> can hold, otherwise a <see cref="None{T}" />.
    /// </returns>
    /// <remarks>
    /// A <see cref="None{T}" /> is returned both when the factory throws and
    /// when it returns a value a <see cref="Some{T}" /> cannot hold. Only the
    /// thrown case is reported to the exception logger configured on
    /// <see cref="MonadOptions" />, because the implicit conversion to
    /// <see cref="Option{T}" /> decides which values a <see cref="Some{T}" />
    /// can hold and returns <see cref="None{T}" /> rather than throwing. That
    /// conversion is applied inside the try, so the two cannot disagree and
    /// nothing it throws escapes.
    /// </remarks>
    public static async Task<Option<T>> TryAsync<T>(
        Func<Task<T>> asyncFactory,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(asyncFactory))]
        string callerArgumentExpression = "") where T : notnull
    {
        try
        {
            return await asyncFactory();
        }
        catch (Exception ex)
        {
            var caller = new CallerInfo(
                callerMemberName,
                callerArgumentExpression,
                callerLineNumber);
            MonadOptions.Current.Log(ex, caller);
            return None<T>();
        }
    }

    /// <summary>Creates a <see cref="Some{T}" /></summary>
    /// <param name="value">The value of the <see cref="Some{T}" /></param>
    /// <typeparam name="T">The option value's type.</typeparam>
    /// <returns>An <see cref="Option{T}" />.</returns>
    /// <exception cref="ArgumentNullException">
    /// The value is null. A <see cref="Some{T}" /> may hold the default of
    /// its type, but never null.
    /// </exception>
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
    /// Returns a <see cref="Some{T}" /> if the value is not null, otherwise
    /// returns a <see cref="None{T}" />.
    /// </returns>
    public static Option<T> FromNullable<T>(T? value)
        where T : struct =>
        value.HasValue ? new Some<T>(value.Value) : new None<T>();

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
