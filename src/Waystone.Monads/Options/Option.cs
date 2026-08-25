namespace Waystone.Monads.Options;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Configs;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>Creates <see cref="Option{T}" /> values</summary>
/// <remarks>
/// The <see cref="Some{T}" /> constructor and the <see cref="None{T}" />
/// instance are both internal, so this class is the only way to build an
/// <see cref="Option{T}" /> from outside the library.
/// </remarks>
#if !DEBUG
[DebuggerStepThrough]
#endif
public static class Option
{
    /// <summary>
    /// Runs a <paramref name="factory" /> and stores its result in an
    /// <see cref="Option{T}" />, turning a throw into a <see cref="None{T}" />.
    /// </summary>
    /// <param name="factory">The method whose result the option will hold.</param>
    /// <param name="callerMemberName">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <param name="callerLineNumber">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <param name="callerArgumentExpression">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <typeparam name="T">The factory return value's type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> if the factory produces a value that a
    /// <see cref="Some{T}" /> can hold, otherwise a <see cref="None{T}" />.
    /// </returns>
    /// <remarks>
    /// A <see cref="None{T}" /> is returned both when the factory throws and
    /// when it returns null. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured. An <see cref="OperationCanceledException" /> is not caught
    /// at all: it leaves this method untouched, so it is neither logged nor
    /// turned into a <see cref="None{T}" />, and the caller observes the
    /// cancellation it asked for. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
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
        catch (Exception ex) when (MonadOptions.Current.Catches(ex))
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
    /// Awaits an <paramref name="asyncFactory" /> and stores its result in an
    /// <see cref="Option{T}" />, turning a throw into a <see cref="None{T}" />.
    /// </summary>
    /// <param name="asyncFactory">
    /// The asynchronous method whose result the option will hold.
    /// </param>
    /// <param name="callerMemberName">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <param name="callerLineNumber">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <param name="callerArgumentExpression">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <typeparam name="T">The async factory return type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> if the factory produces a value that a
    /// <see cref="Some{T}" /> can hold, otherwise a <see cref="None{T}" />.
    /// </returns>
    /// <remarks>
    /// A <see cref="None{T}" /> is returned both when the factory throws and
    /// when it returns null. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured. An <see cref="OperationCanceledException" /> is not caught
    /// at all: it leaves this method untouched, so it is neither logged nor
    /// turned into a <see cref="None{T}" />, and the caller observes the
    /// cancellation it asked for. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
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
        catch (Exception ex) when (MonadOptions.Current.Catches(ex))
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
    /// Runs a <paramref name="factory" /> over a <paramref name="state" /> and
    /// stores its result in an <see cref="Option{T}" />, turning a throw into a
    /// <see cref="None{T}" />.
    /// </summary>
    /// <param name="state">
    /// The value the factory would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="factory">The method whose result the option will hold.</param>
    /// <param name="callerMemberName">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <param name="callerLineNumber">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <param name="callerArgumentExpression">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <typeparam name="TState">
    /// The state's type. It is unconstrained, so a null state is permitted.
    /// </typeparam>
    /// <typeparam name="T">The factory return value's type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> if the factory produces a value that a
    /// <see cref="Some{T}" /> can hold, otherwise a <see cref="None{T}" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="state" /> is handed to the factory rather than
    /// captured by it, so the factory can be <c>static</c> and the call
    /// allocates no closure. A
    /// <see cref="System.Threading.CancellationToken" /> is the state this
    /// exists for.
    /// </para>
    /// <para>
    /// A <see cref="None{T}" /> is returned both when the factory throws and
    /// when it returns null. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured. An <see cref="OperationCanceledException" /> is not caught
    /// at all: it leaves this method untouched, so it is neither logged nor
    /// turned into a <see cref="None{T}" />, and the caller observes the
    /// cancellation it asked for. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
    /// </para>
    /// </remarks>
    public static Option<T> Try<TState, T>(
        TState state,
        Func<TState, T> factory,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(factory))]
        string callerArgumentExpression = "")
        where T : notnull
    {
        try
        {
            return factory(state);
        }
        catch (Exception ex) when (MonadOptions.Current.Catches(ex))
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
    /// Awaits an <paramref name="asyncFactory" /> over a
    /// <paramref name="state" /> and stores its result in an
    /// <see cref="Option{T}" />, turning a throw into a <see cref="None{T}" />.
    /// </summary>
    /// <param name="state">
    /// The value the factory would otherwise capture. It is passed through
    /// unchanged and is never inspected.
    /// </param>
    /// <param name="asyncFactory">
    /// The asynchronous method whose result the option will hold.
    /// </param>
    /// <param name="callerMemberName">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <param name="callerLineNumber">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <param name="callerArgumentExpression">
    /// Compiler-supplied for the exception logger. Do not pass it.
    /// </param>
    /// <typeparam name="TState">
    /// The state's type. It is unconstrained, so a null state is permitted.
    /// </typeparam>
    /// <typeparam name="T">The async factory return type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> if the factory produces a value that a
    /// <see cref="Some{T}" /> can hold, otherwise a <see cref="None{T}" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="state" /> is handed to the factory rather than
    /// captured by it, so the factory can be <c>static</c> and the call
    /// allocates no closure. A
    /// <see cref="System.Threading.CancellationToken" /> is the state this
    /// exists for.
    /// </para>
    /// <para>
    /// A <see cref="None{T}" /> is returned both when the factory throws and
    /// when it returns null. Only the thrown case reaches the exception logger
    /// configured on <see cref="MonadOptions" />, which also writes to the
    /// console while a debugger is attached, whether or not a logger is
    /// configured. An <see cref="OperationCanceledException" /> is not caught
    /// at all: it leaves this method untouched, so it is neither logged nor
    /// turned into a <see cref="None{T}" />, and the caller observes the
    /// cancellation it asked for. Call
    /// <see cref="MonadOptions.UseCancellationAsFailure" /> to catch it like
    /// any other exception.
    /// </para>
    /// </remarks>
    public static async Task<Option<T>> TryAsync<TState, T>(
        TState state,
        Func<TState, Task<T>> asyncFactory,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerArgumentExpression(nameof(asyncFactory))]
        string callerArgumentExpression = "") where T : notnull
    {
        try
        {
            return await asyncFactory(state);
        }
        catch (Exception ex) when (MonadOptions.Current.Catches(ex))
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
    /// <param name="value">The value the option will hold.</param>
    /// <typeparam name="T">The option value's type.</typeparam>
    /// <returns>
    /// An <see cref="Option{T}" /> that is always a <see cref="Some{T}" />. The
    /// static type is <see cref="Option{T}" />, so match on it to reach the
    /// value.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is null. A <see cref="Some{T}" /> may hold the
    /// default of its type, but never null. The <c>notnull</c> constraint makes
    /// this hard to reach rather than impossible, since <c>default!</c> and an
    /// unconstrained caller both get through. Use the implicit conversion on
    /// <see cref="Option{T}" /> instead to turn null into a
    /// <see cref="None{T}" /> rather than a throw.
    /// </exception>
    public static Option<T> Some<T>(T value) where T : notnull =>
        new Some<T>(value);

    /// <summary>Gets the <see cref="None{T}" /> for <typeparamref name="T" /></summary>
    /// <typeparam name="T">The option value's type.</typeparam>
    /// <returns>An <see cref="Option{T}" /> that holds no value.</returns>
    /// <remarks>
    /// A <see cref="None{T}" /> holds nothing, so every call for a given
    /// <typeparamref name="T" /> returns the same cached instance rather than
    /// creating one. Two <see cref="None{T}" /> values compare equal, and
    /// <c>ReferenceEquals</c> answers true.
    /// </remarks>
    public static Option<T> None<T>() where T : notnull =>
        Options.None<T>.Instance;

    /// <summary>Creates an <see cref="Option{T}" /> from a nullable value type.</summary>
    /// <typeparam name="T">
    /// The underlying value type. The parameter is its
    /// <see cref="Nullable{T}" /> form, and the constraint is what selects this
    /// overload over the reference type one.
    /// </typeparam>
    /// <param name="value">
    /// The nullable value to lift into an <see cref="Option{T}" />.
    /// </param>
    /// <returns>
    /// A <see cref="Some{T}" /> holding the value when it has one, otherwise a
    /// <see cref="None{T}" />.
    /// </returns>
    public static Option<T> FromNullable<T>(T? value)
        where T : struct =>
        value.HasValue ? new Some<T>(value.Value) : None<T>();

    /// <summary>Creates an <see cref="Option{T}" /> from a nullable reference type.</summary>
    /// <typeparam name="T">
    /// The reference type. The parameter is its nullable annotation, and the
    /// constraint is what selects this overload over the value type one.
    /// </typeparam>
    /// <param name="value">
    /// The nullable reference to lift into an <see cref="Option{T}" />.
    /// </param>
    /// <returns>
    /// A <see cref="Some{T}" /> holding the value when it is not null,
    /// otherwise a <see cref="None{T}" />.
    /// </returns>
    public static Option<T> FromNullable<T>(T? value)
        where T : class =>
        value is null ? None<T>() : new Some<T>(value);
}
