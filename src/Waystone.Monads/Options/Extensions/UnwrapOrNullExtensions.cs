namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Waystone.SourceGenerators;

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
        /// Prefer this to <see cref="Option{T}.UnwrapOrDefault" /> when
        /// <typeparamref name="T" /> is a value type. <c>UnwrapOrDefault</c> returns the
        /// default of <typeparamref name="T" /> for a <see cref="None{T}" />, which is
        /// indistinguishable from a legitimate zero.
        /// </remarks>
        /// <returns>
        /// The contained value if the option was a <see cref="Some{T}" />,
        /// otherwise <see langword="null" />.
        /// </returns>
        public T? UnwrapOrNull() => option.Match<T?>(value => value, () => null);
    }
}
