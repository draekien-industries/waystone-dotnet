namespace Shouldly;

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Waystone.Monads.Options;

/// <summary>
/// Asserts on the state and contents of an <see cref="Option{T}" />, on either a
/// synchronous receiver or one still inside a task.
/// </summary>
/// <remarks>
/// These report what the option actually was. Reading
/// <see cref="Option{T}.IsSome" /> into a boolean assertion instead costs that: on
/// failure it can only say that true was expected and false found, having thrown
/// away the option before the assertion ran.
/// <para>
/// Every <c>Async</c> overload returns a <see cref="ValueTask{TResult}" /> that has
/// to be awaited. A discarded one never runs its assertion, so the test passes
/// without checking anything — which is why they carry the suffix instead of
/// overloading the synchronous names.
/// </para>
/// </remarks>
[ShouldlyMethods]
[DebuggerStepThrough]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class OptionAssertions
{
    extension<T>(Option<T> actual) where T : notnull
    {
        /// <summary>
        /// Asserts that the option is a <see cref="Some{T}" /> and hands back the
        /// value it holds, so a caller can go on to assert about the value itself.
        /// </summary>
        /// <param name="customMessage">
        /// Extra context to add to the failure, printed under an
        /// <c>Additional Info</c> heading rather than replacing the message.
        /// </param>
        /// <param name="actualExpression">
        /// Filled in by the compiler with the source text of the receiver, so the
        /// failure names the caller's own expression. Do not pass it positionally.
        /// </param>
        /// <returns>The value the option holds.</returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the option is a <see cref="None{T}" />.
        /// </exception>
        public T ShouldBeSome(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null)
        {
            if (actual.IsNone)
            {
                throw new ShouldAssertException(
                    MonadMessage.Build(
                        actualExpression,
                        "Some",
                        MonadMessage.Describe(actual),
                        customMessage));
            }

            return actual.Unwrap();
        }

        /// <summary>
        /// Asserts that the option holds no value.
        /// </summary>
        /// <remarks>
        /// Returns nothing, because the assertion passing means there is no value
        /// to return. This is the only assertion here with no useful result.
        /// </remarks>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler with the source text of the receiver.
        /// </param>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the option is a <see cref="Some{T}" />. The message names
        /// the value found, which is the part a failing boolean assertion cannot
        /// report.
        /// </exception>
        public void ShouldBeNone(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null)
        {
            if (actual.IsSome)
            {
                throw new ShouldAssertException(
                    MonadMessage.Build(
                        actualExpression,
                        "None",
                        MonadMessage.Describe(actual),
                        customMessage));
            }
        }

        /// <summary>
        /// Asserts that the option is a <see cref="Some{T}" /> holding a particular
        /// value, in one call rather than an unwrap followed by a comparison.
        /// </summary>
        /// <remarks>
        /// The two failures are reported differently on purpose. A
        /// <see cref="None{T}" /> is reported here, since the state is the
        /// mismatch; a wrong value is handed to <c>ShouldBe</c>, whose diff on
        /// strings and collections is better than anything restated here.
        /// </remarks>
        /// <param name="expected">
        /// The value the option must hold. Compared through Shouldly, so a string
        /// or a collection compares by content rather than by reference.
        /// </param>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler with the source text of the receiver.
        /// </param>
        /// <returns>
        /// The value the option holds, which equals <paramref name="expected" />.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the option is a <see cref="None{T}" />, and when it holds a
        /// value other than <paramref name="expected" />.
        /// </exception>
        public T ShouldBeSomeValue(
            T expected,
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null)
        {
            if (actual.IsNone)
            {
                throw new ShouldAssertException(
                    MonadMessage.Build(
                        actualExpression,
                        MonadMessage.Some(expected),
                        MonadMessage.Describe(actual),
                        customMessage));
            }

            T value = actual.Unwrap();

            value.ShouldBe(expected, customMessage);

            return value;
        }
    }

    extension<T>(Task<Option<T>> actual) where T : notnull
    {
        /// <summary>
        /// Awaits the task, then asserts that the option it produced is a
        /// <see cref="Some{T}" /> and hands back the value it holds.
        /// </summary>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion, so
        /// the failure names the caller's expression rather than this method's
        /// parameter.
        /// </param>
        /// <returns>
        /// The value the awaited option holds. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited option is a <see cref="None{T}" />.
        /// </exception>
        public async ValueTask<T> ShouldBeSomeAsync(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeSome(
                customMessage,
                actualExpression);

        /// <summary>
        /// Awaits the task, then asserts that the option it produced holds no
        /// value.
        /// </summary>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// A task carrying the assertion. Await it, or the assertion never runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited option is a <see cref="Some{T}" />.
        /// </exception>
        public async ValueTask ShouldBeNoneAsync(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeNone(
                customMessage,
                actualExpression);

        /// <summary>
        /// Awaits the task, then asserts that the option it produced is a
        /// <see cref="Some{T}" /> holding a particular value.
        /// </summary>
        /// <param name="expected">
        /// The value the awaited option must hold. Compared through Shouldly, so a
        /// string or a collection compares by content rather than by reference.
        /// </param>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// The value the awaited option holds. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited option is a <see cref="None{T}" />, and when it
        /// holds a value other than <paramref name="expected" />.
        /// </exception>
        public async ValueTask<T> ShouldBeSomeValueAsync(
            T expected,
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeSomeValue(
                expected,
                customMessage,
                actualExpression);
    }

    extension<T>(ValueTask<Option<T>> actual) where T : notnull
    {
        /// <summary>
        /// Awaits the value task, then asserts that the option it produced is a
        /// <see cref="Some{T}" /> and hands back the value it holds.
        /// </summary>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// The value the awaited option holds. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited option is a <see cref="None{T}" />.
        /// </exception>
        public async ValueTask<T> ShouldBeSomeAsync(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeSome(
                customMessage,
                actualExpression);

        /// <summary>
        /// Awaits the value task, then asserts that the option it produced holds no
        /// value.
        /// </summary>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// A task carrying the assertion. Await it, or the assertion never runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited option is a <see cref="Some{T}" />.
        /// </exception>
        public async ValueTask ShouldBeNoneAsync(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeNone(
                customMessage,
                actualExpression);

        /// <summary>
        /// Awaits the value task, then asserts that the option it produced is a
        /// <see cref="Some{T}" /> holding a particular value.
        /// </summary>
        /// <param name="expected">
        /// The value the awaited option must hold. Compared through Shouldly, so a
        /// string or a collection compares by content rather than by reference.
        /// </param>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// The value the awaited option holds. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited option is a <see cref="None{T}" />, and when it
        /// holds a value other than <paramref name="expected" />.
        /// </exception>
        public async ValueTask<T> ShouldBeSomeValueAsync(
            T expected,
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeSomeValue(
                expected,
                customMessage,
                actualExpression);
    }
}
