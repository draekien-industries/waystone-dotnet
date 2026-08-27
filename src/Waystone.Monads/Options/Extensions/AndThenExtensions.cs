namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Provides <c>AndThenAsync</c> overloads for chaining an
/// <see cref="Option{T}" /> onto a fallible operation, for the cases the
/// synchronous <see cref="Option{T}.AndThen{TOut}" /> cannot cover: an
/// asynchronous factory, a receiver still inside a task, or both.
/// </summary>
/// <remarks>
/// Only the overload below is hand-written. The awaited-receiver generator
/// lifts it, and every overload of <c>Option&lt;T&gt;.AndThen</c>, onto a
/// <see cref="Task{TResult}" /> and a <see cref="ValueTask{TResult}" />
/// receiver.
/// </remarks>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.AndThen))]
public static partial class AndThenExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Chains an <see cref="Option{T}" /> onto an asynchronous fallible
        /// operation, short-circuiting on a <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// <paramref name="optionFactory" /> is not invoked on a <see cref="None{T}" />, so
        /// nothing is awaited on that path. This is the overload to pick over
        /// <see cref="Option{T}.AndThen{TOut}" /> when the chained operation is
        /// itself asynchronous. The result is flat: a <see cref="None{T}" />
        /// returned by <paramref name="optionFactory" /> is indistinguishable from the
        /// short-circuit.
        /// </remarks>
        /// <typeparam name="TOut">The type of the resulting <see cref="Option{TOut}" />.</typeparam>
        /// <param name="optionFactory">
        /// The asynchronous operation to chain onto the contained value.
        /// </param>
        /// <returns>
        /// Whatever <paramref name="optionFactory" /> produced, or a <see cref="None{T}" />
        /// when the option was a <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<Option<TOut>> AndThenAsync<TOut>(
            Func<T, ValueTask<Option<TOut>>> optionFactory)
            where TOut : notnull
        {
            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            Option<TOut> mapped =
                await optionFactory.Invoke(some).ConfigureAwait(false);

            return mapped;
        }
    }
}
