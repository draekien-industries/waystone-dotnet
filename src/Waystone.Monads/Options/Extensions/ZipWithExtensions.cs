namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Applies <c>ZipWith</c> and <c>ZipWithAsync</c> to an
/// <see cref="Option{T}" /> that is still inside a <see cref="Task{TResult}" />
/// or <see cref="ValueTask{TResult}" />, including the shapes where the option
/// being combined with is awaited too.
/// </summary>
/// <remarks>
/// Only the two overloads below are hand-written, and they are the two the
/// generator cannot produce: it lifts a member onto an awaited receiver without
/// touching its parameters, so it has no way to reach a shape where the
/// <c>other</c> argument is itself awaited. Both members named in the
/// attributes are declared on <see cref="Option{T}" /> and generated onto both
/// receivers.
/// </remarks>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.ZipWith))]
[GenerateAwaitedMember(nameof(Option<>.ZipWithAsync))]
public static partial class ZipWithExtensions
{
    extension<TSelf>(Task<Option<TSelf>> optionTask) where TSelf : notnull
    {
        /// <summary>
        /// Awaits two <see cref="Task{TResult}" /> options and combines their
        /// values, when both hold one.
        /// </summary>
        /// <param name="otherTask">The awaited option to combine with.</param>
        /// <param name="zip">
        /// Combines the two contained values. It is invoked only when both options
        /// are a <see cref="Some{T}" />.
        /// </param>
        /// <typeparam name="TOther">The value type of the other option.</typeparam>
        /// <typeparam name="TOut">The type the delegate produces.</typeparam>
        /// <returns>
        /// <see cref="Some{T}" /> of what <paramref name="zip" /> produced when both
        /// options hold a value, otherwise <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<Option<TOut>> ZipWithAsync<TOther, TOut>(
            Task<Option<TOther>> otherTask,
            Func<TSelf, TOther, Task<TOut>> zip)
            where TOther : notnull
            where TOut : notnull
        {
            Option<TSelf> option = await optionTask.ConfigureAwait(false);
            Option<TOther> other = await otherTask.ConfigureAwait(false);

            return await option.ZipWithAsync(other, zip).ConfigureAwait(false);
        }
    }

    extension<TSelf>(ValueTask<Option<TSelf>> optionTask) where TSelf : notnull
    {
        /// <summary>
        /// Awaits two <see cref="ValueTask{TResult}" /> options and combines
        /// their values, when both hold one.
        /// </summary>
        /// <param name="otherTask">The awaited option to combine with.</param>
        /// <param name="zip">
        /// Combines the two contained values. It is invoked only when both options
        /// are a <see cref="Some{T}" />.
        /// </param>
        /// <typeparam name="TOther">The value type of the other option.</typeparam>
        /// <typeparam name="TOut">The type the delegate produces.</typeparam>
        /// <returns>
        /// <see cref="Some{T}" /> of what <paramref name="zip" /> produced when both
        /// options hold a value, otherwise <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<Option<TOut>> ZipWithAsync<TOther, TOut>(
            ValueTask<Option<TOther>> otherTask,
            Func<TSelf, TOther, Task<TOut>> zip)
            where TOther : notnull
            where TOut : notnull
        {
            Option<TSelf> option = await optionTask.ConfigureAwait(false);
            Option<TOther> other = await otherTask.ConfigureAwait(false);

            return await option.ZipWithAsync(other, zip).ConfigureAwait(false);
        }
    }
}
