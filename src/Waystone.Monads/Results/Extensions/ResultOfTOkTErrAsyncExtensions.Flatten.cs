namespace Waystone.Monads.Results.Extensions;

using System.Threading.Tasks;

public static partial class ResultOfTOkTErrAsyncExtensions
{
    /// <summary>
    /// Flattens a <see cref="Task" /> or <see cref="ValueTask" /> containing
    /// a nested <see cref="Result" /> structure into a single <see cref="Result" />.
    /// </summary>
    /// <param name="resultTask">
    /// An asynchronous task or value task producing a nested
    /// <see cref="Result" /> of type <see cref="Result{TOk, TErr}" /> encapsulated
    /// within another <see cref="Result{TOk, TErr}" />.
    /// </param>
    /// <typeparam name="TOk">The type of the successful result.</typeparam>
    /// <typeparam name="TErr">The type of the error.</typeparam>
    /// <returns>
    /// A <see cref="Task" />  that produces a flattened
    /// <see cref="Result{TOk, TErr}" />.
    /// </returns>
    public static Task<Result<TOk, TErr>> FlattenAsync<TOk, TErr>(
        this Task<Result<Result<TOk, TErr>, TErr>> resultTask)
        where TOk : notnull where TErr : notnull =>
        resultTask.MatchAsync(
            Task.FromResult,
            err => Task.FromResult(Result.Err<TOk, TErr>(err)));

    /// <summary>
    /// Flattens a <see cref="ValueTask" /> containing a nested
    /// <see cref="Result" /> structure into a single <see cref="Result" />.
    /// </summary>
    /// <param name="resultTask">
    /// An asynchronous value task producing a nested
    /// <see cref="Result" /> of type <see cref="Result{TOk, TErr}" /> encapsulated
    /// within another <see cref="Result{TOk, TErr}" />.
    /// </param>
    /// <typeparam name="TOk">The type of the successful result.</typeparam>
    /// <typeparam name="TErr">The type of the error.</typeparam>
    /// <returns>
    /// A <see cref="ValueTask" /> that produces a flattened
    /// <see cref="Result{TOk, TErr}" />.
    /// </returns>
    public static ValueTask<Result<TOk, TErr>> FlattenAsync<TOk, TErr>(
        this ValueTask<Result<Result<TOk, TErr>, TErr>> resultTask)
        where TOk : notnull where TErr : notnull =>
        resultTask.MatchAsync(
            inner => new ValueTask<Result<TOk, TErr>>(inner),
            err => new ValueTask<Result<TOk, TErr>>(
                Result.Err<TOk, TErr>(err)));
}
