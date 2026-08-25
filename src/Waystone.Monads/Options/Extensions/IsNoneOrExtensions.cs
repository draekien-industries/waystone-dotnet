namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Asynchronous <c>IsNoneOr</c> extensions for <see cref="Option{T}" />.
/// </summary>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.IsNoneOr))]
public static partial class IsNoneOrExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Checks whether the option is a <see cref="None{T}" />, or awaits
        /// <paramref name="predicate" /> against the contained value if it is a
        /// <see cref="Some{T}" />.
        /// </summary>
        /// <param name="predicate">
        /// An asynchronous condition to evaluate the contained value against. It is
        /// not invoked when the option is a <see cref="None{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing true if the option is a
        /// <see cref="None{T}" /> or the predicate passes for the contained value;
        /// false otherwise.
        /// </returns>
        public async ValueTask<bool> IsNoneOrAsync(
            Func<T, Task<bool>> predicate)
        {
            if (option.IsNone) return true;

            T some = option.Expect("Expected Some but found None.");

            return await predicate.Invoke(some).ConfigureAwait(false);
        }
    }
}
