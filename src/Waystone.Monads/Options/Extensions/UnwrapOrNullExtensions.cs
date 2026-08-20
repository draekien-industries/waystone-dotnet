namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;

public static class UnwrapOrNullExtensions
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

    extension<T>(Task<Option<T>> optionTask) where T : struct
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and returns the contained value if
        /// the option is a <see cref="Some{T}" />, otherwise <see langword="null" />.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the contained value if the
        /// awaited option was a <see cref="Some{T}" />, otherwise
        /// <see langword="null" />.
        /// </returns>
        public async ValueTask<T?> UnwrapOrNullAsync()
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.UnwrapOrNull();
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : struct
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and returns the contained value
        /// if the option is a <see cref="Some{T}" />, otherwise
        /// <see langword="null" />.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the contained value if the
        /// awaited option was a <see cref="Some{T}" />, otherwise
        /// <see langword="null" />.
        /// </returns>
        public async ValueTask<T?> UnwrapOrNullAsync()
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.UnwrapOrNull();
        }
    }
}
