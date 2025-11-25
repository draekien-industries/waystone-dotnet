namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class InspectExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Asynchronously inspects the <see cref="Result{TOk, TErr}" /> if it is
        /// <see cref="Result{TOk, TErr}.IsOk" />
        /// by executing the specified action on the <typeparamref name="TOk" /> value.
        /// </summary>
        /// <param name="action">
        /// A function that represents the asynchronous operation to be performed
        /// on the <typeparamref name="TOk" /> value if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />.
        /// </param>
        /// <returns>
        /// The original <see cref="Result{TOk, TErr}" /> after executing the specified
        /// <paramref name="action" />,
        /// regardless of its state.
        /// </returns>
        public async ValueTask<Result<TOk, TErr>>
            InspectAsync(Func<TOk, Task> action)
        {
            if (result.IsErr) return result;

            TOk ok = result.Expect("Expected Ok but found Err.");

            await action.Invoke(ok).ConfigureAwait(false);

            return result;
        }
    }

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Asynchronously inspects the <see cref="Result{TOk, TErr}" /> if it is
        /// <see cref="Result{TOk, TErr}.IsOk" /> by executing the specified asynchronous
        /// action on the <typeparamref name="TOk" /> value.
        /// </summary>
        /// <param name="action">
        /// A function that represents the asynchronous operation to be performed on the
        /// <typeparamref name="TOk" /> value if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />.
        /// </param>
        /// <returns>
        /// The original <see cref="Result{TOk, TErr}" /> after executing the specified
        /// <paramref name="action" />, regardless of its state.
        /// </returns>
        public async Task<Result<TOk, TErr>> InspectAsync(
            Func<TOk, Task> action)
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            return await result.InspectAsync(action).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously inspects the <see cref="Result{TOk, TErr}" /> contained within
        /// the provided task
        /// if it is <see cref="Result{TOk, TErr}.IsOk" />, by executing the specified
        /// action on the <typeparamref name="TOk" /> value.
        /// </summary>
        /// <param name="action">
        /// A function that represents the operation to be performed asynchronously
        /// on the <typeparamref name="TOk" /> value if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> representing the asynchronous operation.
        /// The task's result is the original <see cref="Result{TOk, TErr}" /> after
        /// executing the specified <paramref name="action" />,
        /// regardless of its state.
        /// </returns>
        public async Task<Result<TOk, TErr>> InspectAsync(Action<TOk> action)
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            return result.Inspect(action);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Asynchronously inspects the <see cref="Result{TOk, TErr}" /> if it is
        /// <see cref="Result{TOk, TErr}.IsOk" /> by executing the specified asynchronous
        /// action on the <typeparamref name="TOk" /> value.
        /// </summary>
        /// <param name="action">
        /// A function that represents the asynchronous operation to be performed on the
        /// <typeparamref name="TOk" /> value if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />.
        /// </param>
        /// <returns>
        /// The original <see cref="Result{TOk, TErr}" /> after executing the specified
        /// <paramref name="action" />, regardless of its state.
        /// </returns>
        public async Task<Result<TOk, TErr>> InspectAsync(
            Func<TOk, Task> action)
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            return await result.InspectAsync(action).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously inspects the <see cref="Result{TOk, TErr}" /> contained within
        /// the provided task
        /// if it is <see cref="Result{TOk, TErr}.IsOk" />, by executing the specified
        /// action on the <typeparamref name="TOk" /> value.
        /// </summary>
        /// <param name="action">
        /// A function that represents the operation to be performed asynchronously
        /// on the <typeparamref name="TOk" /> value if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> representing the asynchronous operation.
        /// The task's result is the original <see cref="Result{TOk, TErr}" /> after
        /// executing the specified <paramref name="action" />,
        /// regardless of its state.
        /// </returns>
        public async Task<Result<TOk, TErr>> InspectAsync(Action<TOk> action)
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            return result.Inspect(action);
        }
    }
}
