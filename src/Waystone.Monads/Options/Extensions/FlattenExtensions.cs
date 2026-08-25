namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;

/// <summary>
/// Flattens a nested <see cref="Option{T}" /> that is still inside a
/// <see cref="Task{TResult}" /> or <see cref="ValueTask{TResult}" />.
/// </summary>
public static class FlattenExtensions
{
    extension<T>(Task<Option<Option<T>>> nestedOptionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and flattens the nested
        /// <see cref="Option{T}" /> into a single-level <see cref="Option{T}" />.
        /// </summary>
        /// <remarks>Flattening only removes one level of nesting at a time.</remarks>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the inner
        /// <see cref="Option{T}" /> if the awaited option was a
        /// <see cref="Some{T}" />, otherwise <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<Option<T>> FlattenAsync()
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
        /// Awaits the <see cref="ValueTask{TResult}" /> and flattens the nested
        /// <see cref="Option{T}" /> into a single-level <see cref="Option{T}" />.
        /// </summary>
        /// <remarks>Flattening only removes one level of nesting at a time.</remarks>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the inner
        /// <see cref="Option{T}" /> if the awaited option was a
        /// <see cref="Some{T}" />, otherwise <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<Option<T>> FlattenAsync()
        {
            Option<Option<T>> nestedOption =
                await nestedOptionTask.ConfigureAwait(false);

            return nestedOption.Flatten();
        }
    }
}
