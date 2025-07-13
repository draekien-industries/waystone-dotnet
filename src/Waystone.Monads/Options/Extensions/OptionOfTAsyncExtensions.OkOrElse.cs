namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Results;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Transforms the current <see cref="Option{T}" /> into a
    /// <see cref="Result{TOk, TErr}" />, mapping <see cref="Some{T}" /> to
    /// <see cref="Ok{TOk, TErr}" /> and <see cref="None{T}" /> to
    /// <see cref="Err{TOk, TErr}" />.
    /// </summary>
    /// <remarks>
    /// The <paramref name="errorFactory" /> is lazily evaluated, meaning it
    /// will only be invoked if the current option is a <see cref="None{T}" />.
    /// </remarks>
    /// <typeparam name="TOk">The type of the value contained inside the option.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <param name="option">The current option.</param>
    /// <param name="errorFactory">The async error factory.</param>
    /// <returns>
    /// An <see cref="Ok{TOk, TErr}" /> if the current option is a
    /// <see cref="Some{T}" />, otherwise an <see cref="Err{TOk, TErr}" />.
    /// </returns>
    public static Task<Result<TOk, TErr>> OkOrElseAsync<TOk, TErr>(
        this Option<TOk> option,
        Func<Task<TErr>> errorFactory)
        where TOk : notnull
        where TErr : notnull => option.MatchAsync(
        some => Task.FromResult(Result.Ok<TOk, TErr>(some)),
        async () =>
        {
            TErr error = await errorFactory().ConfigureAwait(false);
            return Result.Err<TOk, TErr>(error);
        });


    /// <summary>
    /// Transforms the current <see cref="Option{T}" /> into a
    /// <see cref="Result{TOk, TErr}" />, mapping <see cref="Some{T}" /> to
    /// <see cref="Ok{TOk, TErr}" /> and <see cref="None{T}" /> to
    /// <see cref="Err{TOk, TErr}" />.
    /// </summary>
    /// <remarks>
    /// The <paramref name="errorFactory" /> is lazily evaluated, meaning it
    /// will only be invoked if the current option is a <see cref="None{T}" />.
    /// </remarks>
    /// <typeparam name="TOk">The type of the value contained inside the option.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <param name="optionTask">The current option.</param>
    /// <param name="errorFactory">The async error factory.</param>
    /// <returns>
    /// An <see cref="Ok{TOk, TErr}" /> if the current option is a
    /// <see cref="Some{T}" />, otherwise an <see cref="Err{TOk, TErr}" />.
    /// </returns>
    public static async Task<Result<TOk, TErr>> OkOrElseAsync<TOk, TErr>(
        this Task<Option<TOk>> optionTask,
        Func<Task<TErr>> errorFactory)
        where TOk : notnull
        where TErr : notnull
    {
        Option<TOk> option = await optionTask.ConfigureAwait(false);
        return await option.OkOrElseAsync(errorFactory).ConfigureAwait(false);
    }

    /// <summary>
    /// Transforms the current <see cref="Option{T}" /> into a
    /// <see cref="Result{TOk, TErr}" />, mapping <see cref="Some{T}" /> to
    /// <see cref="Ok{TOk, TErr}" /> and <see cref="None{T}" /> to
    /// <see cref="Err{TOk, TErr}" />.
    /// </summary>
    /// <remarks>
    /// The <paramref name="errorFactory" /> is lazily evaluated, meaning it
    /// will only be invoked if the current option is a <see cref="None{T}" />.
    /// </remarks>
    /// <typeparam name="TOk">The type of the value contained inside the option.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <param name="option">The current option.</param>
    /// <param name="errorFactory">The async error factory.</param>
    /// <returns>
    /// An <see cref="Ok{TOk, TErr}" /> if the current option is a
    /// <see cref="Some{T}" />, otherwise an <see cref="Err{TOk, TErr}" />.
    /// </returns>
    public static ValueTask<Result<TOk, TErr>> OkOrElseAsync<TOk, TErr>(
        this Option<TOk> option,
        Func<ValueTask<TErr>> errorFactory)
        where TOk : notnull
        where TErr : notnull => option.MatchAsync(
        some => new ValueTask<Result<TOk, TErr>>(Result.Ok<TOk, TErr>(some)),
        async () =>
        {
            TErr error = await errorFactory().ConfigureAwait(false);
            return Result.Err<TOk, TErr>(error);
        });


    /// <summary>
    /// Transforms the current <see cref="Option{T}" /> into a
    /// <see cref="Result{TOk, TErr}" />, mapping <see cref="Some{T}" /> to
    /// <see cref="Ok{TOk, TErr}" /> and <see cref="None{T}" /> to
    /// <see cref="Err{TOk, TErr}" />.
    /// </summary>
    /// <remarks>
    /// The <paramref name="errorFactory" /> is lazily evaluated, meaning it
    /// will only be invoked if the current option is a <see cref="None{T}" />.
    /// </remarks>
    /// <typeparam name="TOk">The type of the value contained inside the option.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <param name="optionTask">The current option.</param>
    /// <param name="errorFactory">The async error factory.</param>
    /// <returns>
    /// An <see cref="Ok{TOk, TErr}" /> if the current option is a
    /// <see cref="Some{T}" />, otherwise an <see cref="Err{TOk, TErr}" />.
    /// </returns>
    public static async ValueTask<Result<TOk, TErr>> OkOrElseAsync<TOk, TErr>(
        this ValueTask<Option<TOk>> optionTask,
        Func<ValueTask<TErr>> errorFactory)
        where TOk : notnull
        where TErr : notnull
    {
        Option<TOk> option = await optionTask.ConfigureAwait(false);
        return await option.OkOrElseAsync(errorFactory).ConfigureAwait(false);
    }
}
