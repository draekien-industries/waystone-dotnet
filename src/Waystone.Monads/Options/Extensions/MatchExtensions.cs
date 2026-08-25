namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>
/// Provides <c>MatchAsync</c> overloads for an <see cref="Option{T}" /> that is
/// still inside a task.
/// </summary>
public static class MatchExtensions
{
    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits a task of <see cref="Option{T}" /> and switches on it, running
        /// whichever of two asynchronous callbacks matches its case.
        /// </summary>
        /// <remarks>
        /// Exactly one callback runs; the other is never invoked. An exception
        /// from the receiver task propagates before either callback is reached.
        /// </remarks>
        /// <typeparam name="TOut">The type both callbacks produce.</typeparam>
        /// <param name="onSome">
        /// An asynchronous callback for the <see cref="Some{T}" /> case, given the
        /// contained value.
        /// </param>
        /// <param name="onNone">
        /// An asynchronous callback for the <see cref="None{T}" /> case.
        /// </param>
        /// <returns>The awaited output of whichever callback ran.</returns>
        public async ValueTask<TOut> MatchAsync<TOut>(
            Func<T, Task<TOut>> onSome,
            Func<Task<TOut>> onNone)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return await option.Match(onSome, onNone).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits a task of <see cref="Option{T}" /> and switches on it with a
        /// synchronous <see cref="Some{T}" /> callback and an asynchronous
        /// <see cref="None{T}" /> callback.
        /// </summary>
        /// <remarks>
        /// Exactly one callback runs; the other is never invoked. Pick this
        /// overload when only the <see cref="None{T}" /> branch needs to await, so
        /// the <see cref="Some{T}" /> branch is not forced through a task.
        /// </remarks>
        /// <typeparam name="TOut">The type both callbacks produce.</typeparam>
        /// <param name="onSome">
        /// A synchronous callback for the <see cref="Some{T}" /> case, given the
        /// contained value.
        /// </param>
        /// <param name="onNone">
        /// An asynchronous callback for the <see cref="None{T}" /> case.
        /// </param>
        /// <returns>The output of whichever callback ran.</returns>
        public async ValueTask<TOut> MatchAsync<TOut>(
            Func<T, TOut> onSome,
            Func<Task<TOut>> onNone)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return await onNone().ConfigureAwait(false);

            T some = option.Expect("Expected Some but found None.");

            return onSome(some);
        }

        /// <summary>
        /// Awaits a task of <see cref="Option{T}" /> and switches on it with an
        /// asynchronous <see cref="Some{T}" /> callback and a synchronous
        /// <see cref="None{T}" /> callback.
        /// </summary>
        /// <remarks>
        /// Exactly one callback runs; the other is never invoked. Pick this
        /// overload when the fallback is a plain value and only the
        /// <see cref="Some{T}" /> branch needs to await.
        /// </remarks>
        /// <typeparam name="TOut">The type both callbacks produce.</typeparam>
        /// <param name="onSome">
        /// An asynchronous callback for the <see cref="Some{T}" /> case, given the
        /// contained value.
        /// </param>
        /// <param name="onNone">
        /// A synchronous callback for the <see cref="None{T}" /> case.
        /// </param>
        /// <returns>The output of whichever callback ran.</returns>
        public async ValueTask<TOut> MatchAsync<TOut>(
            Func<T, Task<TOut>> onSome,
            Func<TOut> onNone)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return onNone();

            T some = option.Expect("Expected Some but found None.");

            return await onSome(some).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits a task of <see cref="Option{T}" /> and switches on it, running
        /// whichever of two synchronous callbacks matches its case.
        /// </summary>
        /// <remarks>
        /// Exactly one callback runs; the other is never invoked. Only the
        /// receiver is awaited, so pick this overload when neither branch does
        /// asynchronous work.
        /// </remarks>
        /// <typeparam name="TOut">The type both callbacks produce.</typeparam>
        /// <param name="onSome">
        /// A synchronous callback for the <see cref="Some{T}" /> case, given the
        /// contained value.
        /// </param>
        /// <param name="onNone">
        /// A synchronous callback for the <see cref="None{T}" /> case.
        /// </param>
        /// <returns>The output of whichever callback ran.</returns>
        public async ValueTask<TOut> MatchAsync<TOut>(
            Func<T, TOut> onSome,
            Func<TOut> onNone)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.Match(onSome, onNone);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits a value task of <see cref="Option{T}" /> and switches on it,
        /// running whichever of two asynchronous callbacks matches its case.
        /// </summary>
        /// <remarks>
        /// Exactly one callback runs; the other is never invoked. An exception
        /// from the receiver task propagates before either callback is reached.
        /// </remarks>
        /// <typeparam name="TOut">The type both callbacks produce.</typeparam>
        /// <param name="onSome">
        /// An asynchronous callback for the <see cref="Some{T}" /> case, given the
        /// contained value.
        /// </param>
        /// <param name="onNone">
        /// An asynchronous callback for the <see cref="None{T}" /> case.
        /// </param>
        /// <returns>The awaited output of whichever callback ran.</returns>
        public async ValueTask<TOut> MatchAsync<TOut>(
            Func<T, Task<TOut>> onSome,
            Func<Task<TOut>> onNone)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return await option.Match(onSome, onNone).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits a value task of <see cref="Option{T}" /> and switches on it with
        /// a synchronous <see cref="Some{T}" /> callback and an asynchronous
        /// <see cref="None{T}" /> callback.
        /// </summary>
        /// <remarks>
        /// Exactly one callback runs; the other is never invoked. Pick this
        /// overload when only the <see cref="None{T}" /> branch needs to await, so
        /// the <see cref="Some{T}" /> branch is not forced through a task.
        /// </remarks>
        /// <typeparam name="TOut">The type both callbacks produce.</typeparam>
        /// <param name="onSome">
        /// A synchronous callback for the <see cref="Some{T}" /> case, given the
        /// contained value.
        /// </param>
        /// <param name="onNone">
        /// An asynchronous callback for the <see cref="None{T}" /> case.
        /// </param>
        /// <returns>The output of whichever callback ran.</returns>
        public async ValueTask<TOut> MatchAsync<TOut>(
            Func<T, TOut> onSome,
            Func<Task<TOut>> onNone)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return await onNone().ConfigureAwait(false);

            T some = option.Expect("Expected Some but found None.");

            return onSome(some);
        }

        /// <summary>
        /// Awaits a value task of <see cref="Option{T}" /> and switches on it with
        /// an asynchronous <see cref="Some{T}" /> callback and a synchronous
        /// <see cref="None{T}" /> callback.
        /// </summary>
        /// <remarks>
        /// Exactly one callback runs; the other is never invoked. Pick this
        /// overload when the fallback is a plain value and only the
        /// <see cref="Some{T}" /> branch needs to await.
        /// </remarks>
        /// <typeparam name="TOut">The type both callbacks produce.</typeparam>
        /// <param name="onSome">
        /// An asynchronous callback for the <see cref="Some{T}" /> case, given the
        /// contained value.
        /// </param>
        /// <param name="onNone">
        /// A synchronous callback for the <see cref="None{T}" /> case.
        /// </param>
        /// <returns>The output of whichever callback ran.</returns>
        public async ValueTask<TOut> MatchAsync<TOut>(
            Func<T, Task<TOut>> onSome,
            Func<TOut> onNone)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return onNone();

            T some = option.Expect("Expected Some but found None.");

            return await onSome(some).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits a value task of <see cref="Option{T}" /> and switches on it,
        /// running whichever of two synchronous callbacks matches its case.
        /// </summary>
        /// <remarks>
        /// Exactly one callback runs; the other is never invoked. Only the
        /// receiver is awaited, so pick this overload when neither branch does
        /// asynchronous work.
        /// </remarks>
        /// <typeparam name="TOut">The type both callbacks produce.</typeparam>
        /// <param name="onSome">
        /// A synchronous callback for the <see cref="Some{T}" /> case, given the
        /// contained value.
        /// </param>
        /// <param name="onNone">
        /// A synchronous callback for the <see cref="None{T}" /> case.
        /// </param>
        /// <returns>The output of whichever callback ran.</returns>
        public async ValueTask<TOut> MatchAsync<TOut>(
            Func<T, TOut> onSome,
            Func<TOut> onNone)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.Match(onSome, onNone);
        }
    }
}
