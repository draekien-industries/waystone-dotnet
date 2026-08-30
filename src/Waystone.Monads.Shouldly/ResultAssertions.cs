namespace Shouldly;

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Waystone.Monads.Results;

/// <summary>
/// Asserts on the state and contents of a <see cref="Result{TOk,TErr}" />, on
/// either a synchronous receiver or one still inside a task.
/// </summary>
/// <remarks>
/// A failing assertion here names the branch the result was actually on and what it
/// carried. That is worth more on a result than on an option: the error a failing
/// test did not expect is usually the whole explanation, and reading
/// <see cref="Result{TOk, TErr}.IsOk" /> into a boolean assertion discards it.
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
public static class ResultAssertions
{
    extension<TOk, TErr>(Result<TOk, TErr> actual)
        where TOk : notnull
        where TErr : notnull
    {
        /// <summary>
        /// Asserts that the result is an <see cref="Ok{TOk, TErr}" /> and hands back
        /// the value it carries.
        /// </summary>
        /// <param name="customMessage">
        /// Extra context to add to the failure, printed under an
        /// <c>Additional Info</c> heading rather than replacing the message.
        /// </param>
        /// <param name="actualExpression">
        /// Filled in by the compiler with the source text of the receiver, so the
        /// failure names the caller's own expression. Do not pass it positionally.
        /// </param>
        /// <returns>The value the result carries.</returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the result is an <see cref="Err{TOk, TErr}" />. The message
        /// names the error, so a test that failed for an unexpected reason says what
        /// that reason was.
        /// </exception>
        public TOk ShouldBeOk(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null)
        {
            if (actual.IsErr)
            {
                throw new ShouldAssertException(
                    MonadMessage.Build(
                        actualExpression,
                        "Ok",
                        MonadMessage.Describe(actual),
                        customMessage));
            }

            return actual.Unwrap();
        }

        /// <summary>
        /// Asserts that the result is an <see cref="Err{TOk, TErr}" /> and hands
        /// back the error it carries.
        /// </summary>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler with the source text of the receiver.
        /// </param>
        /// <returns>The error the result carries.</returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the result is an <see cref="Ok{TOk, TErr}" />.
        /// </exception>
        public TErr ShouldBeErr(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null)
        {
            if (actual.IsOk)
            {
                throw new ShouldAssertException(
                    MonadMessage.Build(
                        actualExpression,
                        "Err",
                        MonadMessage.Describe(actual),
                        customMessage));
            }

            return actual.UnwrapErr();
        }

        /// <summary>
        /// Asserts that the result is an <see cref="Ok{TOk, TErr}" /> carrying a
        /// particular value, in one call rather than an unwrap followed by a
        /// comparison.
        /// </summary>
        /// <remarks>
        /// An <see cref="Err{TOk, TErr}" /> is reported here, since the branch is
        /// the mismatch; a wrong <c>Ok</c> value is handed to <c>ShouldBe</c>, whose
        /// diff on strings and collections is better than anything restated here.
        /// </remarks>
        /// <param name="expected">
        /// The value the result must carry. Compared through Shouldly, so a string
        /// or a collection compares by content rather than by reference.
        /// </param>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler with the source text of the receiver.
        /// </param>
        /// <returns>
        /// The value the result carries, which equals <paramref name="expected" />.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the result is an <see cref="Err{TOk, TErr}" />, and when it
        /// carries a value other than <paramref name="expected" />.
        /// </exception>
        public TOk ShouldBeOkValue(
            TOk expected,
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null)
        {
            if (actual.IsErr)
            {
                throw new ShouldAssertException(
                    MonadMessage.Build(
                        actualExpression,
                        MonadMessage.Ok(expected),
                        MonadMessage.Describe(actual),
                        customMessage));
            }

            TOk value = actual.Unwrap();

            value.ShouldBe(expected, customMessage);

            return value;
        }

        /// <summary>
        /// Asserts that the result is an <see cref="Err{TOk, TErr}" /> carrying a
        /// particular error.
        /// </summary>
        /// <remarks>
        /// The mirror of <c>ShouldBeOkValue</c>: it is the <c>Ok</c> branch that
        /// this one reports, naming the value that arrived where an error was
        /// expected.
        /// </remarks>
        /// <param name="expected">
        /// The error the result must carry. Compared through Shouldly, so a string
        /// or a collection compares by content rather than by reference.
        /// </param>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler with the source text of the receiver.
        /// </param>
        /// <returns>
        /// The error the result carries, which equals <paramref name="expected" />.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the result is an <see cref="Ok{TOk, TErr}" />, and when it
        /// carries an error other than <paramref name="expected" />.
        /// </exception>
        public TErr ShouldBeErrValue(
            TErr expected,
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null)
        {
            if (actual.IsOk)
            {
                throw new ShouldAssertException(
                    MonadMessage.Build(
                        actualExpression,
                        MonadMessage.Err(expected),
                        MonadMessage.Describe(actual),
                        customMessage));
            }

            TErr error = actual.UnwrapErr();

            error.ShouldBe(expected, customMessage);

            return error;
        }
    }

    extension<TOk, TErr>(Task<Result<TOk, TErr>> actual)
        where TOk : notnull
        where TErr : notnull
    {
        /// <summary>
        /// Awaits the task, then asserts that the result it produced is an
        /// <see cref="Ok{TOk, TErr}" /> and hands back the value it carries.
        /// </summary>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion, so
        /// the failure names the caller's expression rather than this method's
        /// parameter.
        /// </param>
        /// <returns>
        /// The value the awaited result carries. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited result is an <see cref="Err{TOk, TErr}" />.
        /// </exception>
        public async ValueTask<TOk> ShouldBeOkAsync(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeOk(
                customMessage,
                actualExpression);

        /// <summary>
        /// Awaits the task, then asserts that the result it produced is an
        /// <see cref="Err{TOk, TErr}" /> and hands back the error it carries.
        /// </summary>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// The error the awaited result carries. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited result is an <see cref="Ok{TOk, TErr}" />.
        /// </exception>
        public async ValueTask<TErr> ShouldBeErrAsync(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeErr(
                customMessage,
                actualExpression);

        /// <summary>
        /// Awaits the task, then asserts that the result it produced is an
        /// <see cref="Ok{TOk, TErr}" /> carrying a particular value.
        /// </summary>
        /// <param name="expected">
        /// The value the awaited result must carry. Compared through Shouldly, so a
        /// string or a collection compares by content rather than by reference.
        /// </param>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// The value the awaited result carries. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited result is an <see cref="Err{TOk, TErr}" />, and
        /// when it carries a value other than <paramref name="expected" />.
        /// </exception>
        public async ValueTask<TOk> ShouldBeOkValueAsync(
            TOk expected,
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeOkValue(
                expected,
                customMessage,
                actualExpression);

        /// <summary>
        /// Awaits the task, then asserts that the result it produced is an
        /// <see cref="Err{TOk, TErr}" /> carrying a particular error.
        /// </summary>
        /// <param name="expected">
        /// The error the awaited result must carry. Compared through Shouldly, so a
        /// string or a collection compares by content rather than by reference.
        /// </param>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// The error the awaited result carries. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited result is an <see cref="Ok{TOk, TErr}" />, and
        /// when it carries an error other than <paramref name="expected" />.
        /// </exception>
        public async ValueTask<TErr> ShouldBeErrValueAsync(
            TErr expected,
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeErrValue(
                expected,
                customMessage,
                actualExpression);
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> actual)
        where TOk : notnull
        where TErr : notnull
    {
        /// <summary>
        /// Awaits the value task, then asserts that the result it produced is an
        /// <see cref="Ok{TOk, TErr}" /> and hands back the value it carries.
        /// </summary>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// The value the awaited result carries. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited result is an <see cref="Err{TOk, TErr}" />.
        /// </exception>
        public async ValueTask<TOk> ShouldBeOkAsync(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeOk(
                customMessage,
                actualExpression);

        /// <summary>
        /// Awaits the value task, then asserts that the result it produced is an
        /// <see cref="Err{TOk, TErr}" /> and hands back the error it carries.
        /// </summary>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// The error the awaited result carries. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited result is an <see cref="Ok{TOk, TErr}" />.
        /// </exception>
        public async ValueTask<TErr> ShouldBeErrAsync(
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeErr(
                customMessage,
                actualExpression);

        /// <summary>
        /// Awaits the value task, then asserts that the result it produced is an
        /// <see cref="Ok{TOk, TErr}" /> carrying a particular value.
        /// </summary>
        /// <param name="expected">
        /// The value the awaited result must carry. Compared through Shouldly, so a
        /// string or a collection compares by content rather than by reference.
        /// </param>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// The value the awaited result carries. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited result is an <see cref="Err{TOk, TErr}" />, and
        /// when it carries a value other than <paramref name="expected" />.
        /// </exception>
        public async ValueTask<TOk> ShouldBeOkValueAsync(
            TOk expected,
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeOkValue(
                expected,
                customMessage,
                actualExpression);

        /// <summary>
        /// Awaits the value task, then asserts that the result it produced is an
        /// <see cref="Err{TOk, TErr}" /> carrying a particular error.
        /// </summary>
        /// <param name="expected">
        /// The error the awaited result must carry. Compared through Shouldly, so a
        /// string or a collection compares by content rather than by reference.
        /// </param>
        /// <param name="customMessage">Extra context to add to the failure.</param>
        /// <param name="actualExpression">
        /// Filled in by the compiler and forwarded to the synchronous assertion.
        /// </param>
        /// <returns>
        /// The error the awaited result carries. Await this, or the assertion never
        /// runs.
        /// </returns>
        /// <exception cref="ShouldAssertException">
        /// Thrown when the awaited result is an <see cref="Ok{TOk, TErr}" />, and
        /// when it carries an error other than <paramref name="expected" />.
        /// </exception>
        public async ValueTask<TErr> ShouldBeErrValueAsync(
            TErr expected,
            string? customMessage = null,
            [CallerArgumentExpression(nameof(actual))]
            string? actualExpression = null) =>
            (await actual.ConfigureAwait(false)).ShouldBeErrValue(
                expected,
                customMessage,
                actualExpression);
    }
}
