namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Provides <c>IsSomeAndAsync</c> overloads for testing an
/// <see cref="Option{T}" /> with an asynchronous predicate, a receiver still
/// inside a task, or both.
/// </summary>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.IsSomeAnd))]
public static partial class IsSomeAndExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Checks whether an <see cref="Option{T}" /> is a <see cref="Some{T}" />
        /// whose value satisfies an asynchronous predicate.
        /// </summary>
        /// <remarks>
        /// The predicate is not invoked when the option is a
        /// <see cref="None{T}" />.
        /// </remarks>
        /// <param name="predicate">
        /// The asynchronous condition to evaluate against the contained value.
        /// </param>
        /// <returns>
        /// True if the option is a <see cref="Some{T}" /> and the predicate returns
        /// true; false otherwise.
        /// </returns>
        public async ValueTask<bool> IsSomeAndAsync(
            Func<T, Task<bool>> predicate)
        {
            if (option.IsNone) return false;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false);
        }
    }
}
