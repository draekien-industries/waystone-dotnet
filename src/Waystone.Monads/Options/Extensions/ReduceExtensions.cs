namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.Reduce))]
public static partial class ReduceExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Merges the option with another option, awaiting
        /// <paramref name="reduce" /> when both are a <see cref="Some{T}" />.
        /// </summary>
        /// <param name="other">The option to merge with.</param>
        /// <param name="reduce">
        /// A function that asynchronously combines two present
        /// values.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the combined option when both
        /// are a <see cref="Some{T}" />, whichever option is a <see cref="Some{T}" /> when
        /// only one of them is, and <see cref="None{T}" /> when neither is.
        /// </returns>
        public async ValueTask<Option<T>> ReduceAsync(
            Option<T> other,
            Func<T, T, Task<T>> reduce)
        {
            if (option.IsNone) return other;

            if (other.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");
            T otherSome = other.Expect("Expected Some but found None.");

            return Option.NoneIfNull(
                await reduce.Invoke(some, otherSome).ConfigureAwait(false));
        }
    }
}
