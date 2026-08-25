namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Unwraps an <see cref="Option{T}" /> of a value type to a nullable, including
/// one still inside a <see cref="Task{TResult}" /> or
/// <see cref="ValueTask{TResult}" />.
/// </summary>
[GenerateAwaitedReceivers(typeof(Option<>))]
public static partial class UnwrapOrNullExtensions
{
    extension<T>(Option<T> option) where T : struct
    {
        /// <summary>
        /// Returns the contained value if the option is a <see cref="Some{T}" />,
        /// otherwise <see langword="null" />.
        /// </summary>
        /// <remarks>
        /// Prefer this to <see cref="Option{T}.UnwrapOrDefault" />, which returns
        /// the default of <typeparamref name="T" /> for a <see cref="None{T}" /> —
        /// for a value type that is indistinguishable from a legitimate zero.
        /// </remarks>
        /// <returns>
        /// The contained value if the option was a <see cref="Some{T}" />,
        /// otherwise <see langword="null" />.
        /// </returns>
        public T? UnwrapOrNull() => option.Match<T?>(value => value, () => null);
    }
}
