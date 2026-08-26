namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>
/// Provides <c>AndThenAsync</c> overloads for chaining an
/// <see cref="Option{T}" /> onto a fallible operation, for the cases the
/// synchronous <see cref="Option{T}.AndThen{TOut}" /> cannot cover: an
/// asynchronous factory, a receiver still inside a task, or both.
/// </summary>
public static class AndThenExtensions
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

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits a task of <see cref="Option{T}" /> and chains it onto an
        /// asynchronous fallible operation, short-circuiting on a
        /// <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// <paramref name="optionFactory" /> is not invoked on a <see cref="None{T}" />, so
        /// that path awaits the receiver only. The result is flat: a
        /// <see cref="None{T}" /> returned by <paramref name="optionFactory" /> is
        /// indistinguishable from the short-circuit.
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
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            Option<TOut> mapped =
                await optionFactory.Invoke(some).ConfigureAwait(false);

            return mapped;
        }

        /// <summary>
        /// Awaits a task of <see cref="Option{T}" /> and chains it onto a
        /// synchronous fallible operation, short-circuiting on a
        /// <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// <paramref name="optionFactory" /> is not invoked on a <see cref="None{T}" />. Only
        /// the receiver is awaited, so pick this overload when the chained
        /// operation does no asynchronous work.
        /// </remarks>
        /// <typeparam name="TOut">The type of the resulting <see cref="Option{TOut}" />.</typeparam>
        /// <param name="optionFactory">
        /// The operation to chain onto the contained value.
        /// </param>
        /// <returns>
        /// Whatever <paramref name="optionFactory" /> produced, or a <see cref="None{T}" />
        /// when the option was a <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<Option<TOut>> AndThenAsync<TOut>(
            Func<T, Option<TOut>> optionFactory)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            Option<TOut> mapped = optionFactory.Invoke(some);

            return mapped;
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits a value task of <see cref="Option{T}" /> and chains it onto an
        /// asynchronous fallible operation, short-circuiting on a
        /// <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// <paramref name="optionFactory" /> is not invoked on a <see cref="None{T}" />, so
        /// that path awaits the receiver only. The result is flat: a
        /// <see cref="None{T}" /> returned by <paramref name="optionFactory" /> is
        /// indistinguishable from the short-circuit.
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
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            Option<TOut> mapped =
                await optionFactory.Invoke(some).ConfigureAwait(false);

            return mapped;
        }

        /// <summary>
        /// Awaits a value task of <see cref="Option{T}" /> and chains it onto a
        /// synchronous fallible operation, short-circuiting on a
        /// <see cref="None{T}" />.
        /// </summary>
        /// <remarks>
        /// <paramref name="optionFactory" /> is not invoked on a <see cref="None{T}" />. Only
        /// the receiver is awaited, so pick this overload when the chained
        /// operation does no asynchronous work.
        /// </remarks>
        /// <typeparam name="TOut">The type of the resulting <see cref="Option{TOut}" />.</typeparam>
        /// <param name="optionFactory">
        /// The operation to chain onto the contained value.
        /// </param>
        /// <returns>
        /// Whatever <paramref name="optionFactory" /> produced, or a <see cref="None{T}" />
        /// when the option was a <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<Option<TOut>> AndThenAsync<TOut>(
            Func<T, Option<TOut>> optionFactory)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            if (option.IsNone) return Option.None<TOut>();

            T some = option.Expect("Expected Some but found None.");

            Option<TOut> mapped = optionFactory.Invoke(some);

            return mapped;
        }
    }
}
