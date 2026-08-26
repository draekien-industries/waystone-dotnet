namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Filters an <see cref="Option{T}" /> with an asynchronous predicate, and filters
/// an <see cref="Option{T}" /> that is still inside a <see cref="Task{TResult}" />
/// or <see cref="ValueTask{TResult}" />.
/// </summary>
/// <remarks>
/// Only the overload below is hand-written. The awaited-receiver generator
/// lifts it, and every overload of <c>Option&lt;T&gt;.Filter</c>, onto a
/// <see cref="Task{TResult}" /> and a <see cref="ValueTask{TResult}" />
/// receiver.
/// </remarks>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.Filter))]
public static partial class FilterExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Awaits <paramref name="predicate" /> against the contained value if the
        /// option is a <see cref="Some{T}" />, keeping the option when it passes.
        /// </summary>
        /// <param name="predicate">
        /// The asynchronous condition the contained value must satisfy.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the original option if it
        /// was a <see cref="Some{T}" /> whose value satisfies
        /// <paramref name="predicate" />, otherwise <see cref="None{T}" />. A
        /// <see cref="None{T}" /> passes through unchanged and the predicate is not
        /// invoked.
        /// </returns>
        public async ValueTask<Option<T>>
            FilterAsync(Func<T, Task<bool>> predicate)
        {
            if (option.IsNone) return option;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false)
                ? option
                : Option.None<T>();
        }
    }
}
