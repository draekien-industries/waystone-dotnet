namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Flattens a nested <see cref="Option{T}" /> structure into a single
    /// <see cref="Option{T}" /> by extracting and combining the values.
    /// </summary>
    /// <param name="nestedOptionTask">
    /// A task containing a nested
    /// <see cref="Option{T}" /> structure that needs to be flattened.
    /// </param>
    /// <typeparam name="T">
    /// The type of the value contained within the
    /// <see cref="Option{T}" />.
    /// </typeparam>
    /// <returns>A task containing the flattened <see cref="Option{T}" /> structure.</returns>
    public static async Task<Option<T>> FlattenAsync<T>(
        this Task<Option<Option<T>>> nestedOptionTask) where T : notnull
    {
        Option<Option<T>> nestedOption =
            await nestedOptionTask.ConfigureAwait(false);
        return nestedOption.Flatten();
    }

    /// <summary>
    /// Flattens a nested <see cref="Option{T}" /> structure into a single
    /// <see cref="Option{T}" /> by extracting and combining the values.
    /// </summary>
    /// <param name="nestedOptionTask">
    /// A value task containing a nested
    /// <see cref="Option{T}" /> structure that needs to be flattened.
    /// </param>
    /// <typeparam name="T">
    /// The type of the value contained within the
    /// <see cref="Option{T}" />.
    /// </typeparam>
    /// <returns>
    /// A value task containing the flattened <see cref="Option{T}" />
    /// structure.
    /// </returns>
    public static async ValueTask<Option<T>> FlattenAsync<T>(
        this ValueTask<Option<Option<T>>> nestedOptionTask) where T : notnull
    {
        Option<Option<T>> nestedOption =
            await nestedOptionTask.ConfigureAwait(false);
        return nestedOption.Flatten();
    }
}
