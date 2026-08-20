namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Exceptions;

public static class UnwrapExtensions
{
    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Asynchronously awaits the <see cref="Option{T}" /> then returns the
        /// contained <see cref="Some{T}" /> value.
        /// </summary>
        /// <exception cref="UnwrapException">
        /// Thrown when the awaited option is a
        /// <see cref="None{T}" />
        /// </exception>
        public async ValueTask<T> UnwrapAsync()
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.Unwrap();
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Option{T}" /> then returns the
        /// contained <see cref="Some{T}" /> value, or the provided default when it is a
        /// <see cref="None{T}" />.
        /// </summary>
        /// <param name="value">The value to return when the option is none.</param>
        public async ValueTask<T> UnwrapOrAsync(T value)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.UnwrapOr(value);
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Option{T}" /> then returns the
        /// contained <see cref="Some{T}" /> value, or the default value of
        /// <typeparamref name="T" /> when it is a <see cref="None{T}" />.
        /// </summary>
        public async ValueTask<T?> UnwrapOrDefaultAsync()
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.UnwrapOrDefault();
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Asynchronously awaits the <see cref="Option{T}" /> then returns the
        /// contained <see cref="Some{T}" /> value.
        /// </summary>
        /// <exception cref="UnwrapException">
        /// Thrown when the awaited option is a
        /// <see cref="None{T}" />
        /// </exception>
        public async ValueTask<T> UnwrapAsync()
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.Unwrap();
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Option{T}" /> then returns the
        /// contained <see cref="Some{T}" /> value, or the provided default when it is a
        /// <see cref="None{T}" />.
        /// </summary>
        /// <param name="value">The value to return when the option is none.</param>
        public async ValueTask<T> UnwrapOrAsync(T value)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.UnwrapOr(value);
        }

        /// <summary>
        /// Asynchronously awaits the <see cref="Option{T}" /> then returns the
        /// contained <see cref="Some{T}" /> value, or the default value of
        /// <typeparamref name="T" /> when it is a <see cref="None{T}" />.
        /// </summary>
        public async ValueTask<T?> UnwrapOrDefaultAsync()
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.UnwrapOrDefault();
        }
    }
}
