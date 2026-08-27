namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Provides <c>MatchAsync</c> overloads for an <see cref="Option{T}" /> with an
/// asynchronous branch, and for one that is still inside a task.
/// </summary>
/// <remarks>
/// Each branch can be synchronous or asynchronous independently, so a caller
/// awaits only the branch that does work. Picking the overload that matches keeps
/// the other branch off the task machinery entirely rather than wrapping a ready
/// value in a completed task.
/// </remarks>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.Match))]
public static partial class MatchExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Switches on an <see cref="Option{T}" /> when both branches do
        /// asynchronous work.
        /// </summary>
        /// <remarks>
        /// Exactly one callback runs and is awaited; the other is never invoked, so
        /// the branch that does not match costs nothing.
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
            Func<Task<TOut>> onNone) =>
            await option.Match(onSome, onNone).ConfigureAwait(false);

        /// <summary>
        /// Switches on an <see cref="Option{T}" /> when only the fallback does
        /// asynchronous work.
        /// </summary>
        /// <remarks>
        /// Exactly one callback runs; the other is never invoked. Reach for this
        /// when the contained value needs no further work to map — fetching a
        /// default from a database, say, where the value already in hand does not
        /// need one.
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
            if (option.IsNone) return await onNone().ConfigureAwait(false);

            T some = option.Expect("Expected Some but found None.");

            return onSome(some);
        }

        /// <summary>
        /// Switches on an <see cref="Option{T}" /> when only the mapping does
        /// asynchronous work.
        /// </summary>
        /// <remarks>
        /// Exactly one callback runs; the other is never invoked. This is the
        /// common shape: the value is worth an asynchronous call and the fallback
        /// is a constant, which stays a constant rather than becoming a completed
        /// task.
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
            if (option.IsNone) return onNone();

            T some = option.Expect("Expected Some but found None.");

            return await onSome(some).ConfigureAwait(false);
        }
    }
}
