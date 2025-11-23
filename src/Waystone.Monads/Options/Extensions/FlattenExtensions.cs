namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;

public static class FlattenExtensions
{
    extension<T>(Task<Option<Option<T>>> nestedOptionTask) where T : notnull
    {
        /// <summary>
        /// Flattens a nested <see cref="Option{T}" /> instance wrapped in a
        /// <see cref="Task{TResult}" /> into a single-level <see cref="Option{T}" />.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value contained in the
        /// <see cref="Option{T}" />.
        /// </typeparam>
        /// <returns>
        /// A task that represents the result of flattening the nested
        /// <see cref="Option{T}" /> to a single-level
        /// <see cref="Option{T}" />.
        /// </returns>
        public async Task<Option<T>> FlattenAsync()
        {
            Option<Option<T>> nestedOption =
                await nestedOptionTask.ConfigureAwait(false);

            return nestedOption.Flatten();
        }
    }

    extension<T>(ValueTask<Option<Option<T>>> nestedOptionTask)
        where T : notnull
    {
        /// <summary>
        /// Flattens an asynchronous nested <see cref="Option{T}" /> instance wrapped in a
        /// <see cref="ValueTask{TResult}" /> into a single-level <see cref="Option{T}" />.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value contained in the <see cref="Option{T}" />.
        /// </typeparam>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> that represents the result of flattening
        /// the
        /// nested <see cref="Option{T}" /> into a single-level <see cref="Option{T}" />.
        /// </returns>
        public async Task<Option<T>> FlattenAsync()
        {
            Option<Option<T>> nestedOption =
                await nestedOptionTask.ConfigureAwait(false);

            return nestedOption.Flatten();
        }
    }
}
