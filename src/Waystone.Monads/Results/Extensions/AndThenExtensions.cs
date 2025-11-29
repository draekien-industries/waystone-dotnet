namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

public static class AndThenExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Executes the provided asynchronous function if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />,
        /// and returns the resulting <see cref="Result{TOut, TErr}" />.
        /// If the result is <see cref="Result{TOk, TErr}.IsErr" />, it propagates the
        /// error.
        /// </summary>
        /// <typeparam name="TOut">
        /// The type of the success value in the resulting
        /// <see cref="Result{TOut, TErr}" />.
        /// </typeparam>
        /// <param name="factory">
        /// A function to be executed if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />.
        /// It takes the success value of the original result as input and returns a
        /// <see cref="Task{TResult}" />
        /// that completes with a <see cref="Result{TOut, TErr}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> that completes with a
        /// <see cref="Result{TOut, TErr}" />.
        /// If the original result is <see cref="Result{TOk, TErr}.IsErr" />, the error is
        /// propagated.
        /// </returns>
        public async ValueTask<Result<TOut, TErr>>
            AndThenAsync<TOut>(Func<TOk, Task<Result<TOut, TErr>>> factory)
            where TOut : notnull
        {
            if (result.IsErr)
            {
                TErr err = result.ExpectErr("Expected Err but found Ok.");

                return Result.Err<TOut, TErr>(err);
            }

            TOk ok = result.Expect("Expected Ok but found Err.");

            return await factory.Invoke(ok).ConfigureAwait(false);
        }
    }

    extension<TOk, TErr>(Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Executes the provided asynchronous function if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />, and returns the resulting
        /// <see cref="Result{TOut, TErr}" />.
        /// If the result is <see cref="Result{TOk, TErr}.IsErr" />, it propagates the
        /// error.
        /// </summary>
        /// <typeparam name="TOut">
        /// The type of the success value in the resulting
        /// <see cref="Result{TOut, TErr}" />.
        /// </typeparam>
        /// <param name="factory">
        /// A function to be executed if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />.
        /// It takes the success value of the original result as input and returns a
        /// <see cref="Task{TResult}" /> that completes with a
        /// <see cref="Result{TOut, TErr}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> that completes with a
        /// <see cref="Result{TOut, TErr}" />.
        /// If the original result is <see cref="Result{TOk, TErr}.IsErr" />, the error is
        /// propagated.
        /// </returns>
        public async Task<Result<TOut, TErr>> AndThenAsync<TOut>(
            Func<TOk, Task<Result<TOut, TErr>>> factory) where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.AndThenAsync(factory).ConfigureAwait(false);
        }

        /// <summary>
        /// Invokes the provided synchronous function if the result of the asynchronous
        /// operation
        /// is <see cref="Result{TOk, TErr}.IsOk" />, and returns the resulting
        /// <see cref="Result{TOut, TErr}" />.
        /// If the result is <see cref="Result{TOk, TErr}.IsErr" />, it propagates the
        /// error.
        /// </summary>
        /// <typeparam name="TOut">
        /// The type of the success value in the resulting
        /// <see cref="Result{TOut, TErr}" />.
        /// </typeparam>
        /// <param name="factory">
        /// A function to be executed if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />.
        /// It takes the success value of the original result as input and returns a
        /// <see cref="Result{TOut, TErr}" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> that completes with a
        /// <see cref="Result{TOut, TErr}" />.
        /// If the original result is <see cref="Result{TOk, TErr}.IsErr" />, the error is
        /// propagated.
        /// </returns>
        public async Task<Result<TOut, TErr>> AndThenAsync<TOut>(
            Func<TOk, Result<TOut, TErr>> factory) where TOut : notnull
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            return result.AndThen(factory);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Executes the provided asynchronous function if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />, and returns the resulting
        /// <see cref="Result{TOut, TErr}" />. If the result is
        /// <see cref="Result{TOk, TErr}.IsErr" />, it propagates the error.
        /// </summary>
        /// <typeparam name="TOut">
        /// The type of the success value in the resulting
        /// <see cref="Result{TOut, TErr}" />.
        /// </typeparam>
        /// <param name="factory">
        /// A function to be executed if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />.
        /// It takes the success value of the original result as input and returns a
        /// <see cref="Task{TResult}" /> that completes with a
        /// <see cref="Result{TOut, TErr}" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> that completes with a
        /// <see cref="Result{TOut, TErr}" />.
        /// If the original result is <see cref="Result{TOk, TErr}.IsErr" />, the error is
        /// propagated.
        /// </returns>
        public async Task<Result<TOut, TErr>> AndThenAsync<TOut>(
            Func<TOk, Task<Result<TOut, TErr>>> factory) where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.AndThenAsync(factory).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the provided asynchronous function if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />, and returns the resulting
        /// <see cref="Result{TOut, TErr}" />. If the result is
        /// <see cref="Result{TOk, TErr}.IsErr" />, it propagates the error.
        /// </summary>
        /// <typeparam name="TOut">
        /// The type of the success value in the resulting
        /// <see cref="Result{TOut, TErr}" />.
        /// </typeparam>
        /// <param name="factory">
        /// A function to be executed if the result is
        /// <see cref="Result{TOk, TErr}.IsOk" />. It takes the success value
        /// of the original result as input and returns a
        /// <see cref="Result{TOut, TErr}" />.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> that completes with a
        /// <see cref="Result{TOut, TErr}" />. If the original result is
        /// <see cref="Result{TOk, TErr}.IsErr" />, the error is propagated.
        /// </returns>
        public async Task<Result<TOut, TErr>> AndThenAsync<TOut>(
            Func<TOk, Result<TOut, TErr>> factory) where TOut : notnull
        {
            Result<TOk, TErr>? result = await resultTask.ConfigureAwait(false);

            return result.AndThen(factory);
        }
    }
}
