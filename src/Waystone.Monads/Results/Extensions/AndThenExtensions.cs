namespace Waystone.Monads.Results.Extensions;

using System;
using System.Threading.Tasks;

/// <summary>
/// Asynchronous <c>AndThen</c> extensions for <see cref="Result{TOk,TErr}" />
/// and for tasks producing one.
/// </summary>
public static class AndThenExtensions
{
    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Calls <paramref name="factory" /> with the contained value if the result
        /// is an <see cref="Ok{TOk,TErr}" /> and awaits it, otherwise propagates the
        /// error.
        /// </summary>
        /// <remarks>
        /// <paramref name="factory" /> is not invoked for an
        /// <see cref="Err{TOk,TErr}" />. Any exception the returned task faults with
        /// surfaces to the caller unchanged.
        /// </remarks>
        /// <typeparam name="TOut">
        /// The ok value type of the result produced by <paramref name="factory" />.
        /// </typeparam>
        /// <param name="factory">
        /// Produces the next result from the contained ok value.
        /// </param>
        /// <returns>
        /// The result <paramref name="factory" /> produced, or the original error
        /// re-wrapped as an <see cref="Err{TOk,TErr}" /> of
        /// <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Result<TOut, TErr>>
            AndThenAsync<TOut>(Func<TOk, ValueTask<Result<TOut, TErr>>> factory)
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
        /// Awaits the receiver, then calls the asynchronous
        /// <paramref name="factory" /> with the contained value if the result is an
        /// <see cref="Ok{TOk,TErr}" />, otherwise propagates the error.
        /// </summary>
        /// <remarks>
        /// <paramref name="factory" /> is not invoked for an
        /// <see cref="Err{TOk,TErr}" />. The receiver is awaited first, so a faulted
        /// receiver task throws before <paramref name="factory" /> is reached.
        /// </remarks>
        /// <typeparam name="TOut">
        /// The ok value type of the result produced by <paramref name="factory" />.
        /// </typeparam>
        /// <param name="factory">
        /// Produces the next result from the contained ok value.
        /// </param>
        /// <returns>
        /// The result <paramref name="factory" /> produced, or the original error
        /// re-wrapped as an <see cref="Err{TOk,TErr}" /> of
        /// <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Result<TOut, TErr>> AndThenAsync<TOut>(
            Func<TOk, ValueTask<Result<TOut, TErr>>> factory)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.AndThenAsync(factory).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits the receiver, then calls the synchronous
        /// <paramref name="factory" /> with the contained value if the result is an
        /// <see cref="Ok{TOk,TErr}" />, otherwise propagates the error.
        /// </summary>
        /// <remarks>
        /// Pick this over the sibling taking a <see cref="Task{TResult}" />-returning
        /// factory when the continuation does no I/O; it avoids a second await.
        /// <paramref name="factory" /> is not invoked for an
        /// <see cref="Err{TOk,TErr}" />.
        /// </remarks>
        /// <typeparam name="TOut">
        /// The ok value type of the result produced by <paramref name="factory" />.
        /// </typeparam>
        /// <param name="factory">
        /// Produces the next result from the contained ok value.
        /// </param>
        /// <returns>
        /// The result <paramref name="factory" /> produced, or the original error
        /// re-wrapped as an <see cref="Err{TOk,TErr}" /> of
        /// <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Result<TOut, TErr>> AndThenAsync<TOut>(
            Func<TOk, Result<TOut, TErr>> factory) where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.AndThen(factory);
        }
    }

    extension<TOk, TErr>(ValueTask<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> receiver, then calls the
        /// asynchronous <paramref name="factory" /> with the contained value if the
        /// result is an <see cref="Ok{TOk,TErr}" />, otherwise propagates the error.
        /// </summary>
        /// <remarks>
        /// The receiver is awaited once, so it must not have been awaited already.
        /// <paramref name="factory" /> is not invoked for an
        /// <see cref="Err{TOk,TErr}" />.
        /// </remarks>
        /// <typeparam name="TOut">
        /// The ok value type of the result produced by <paramref name="factory" />.
        /// </typeparam>
        /// <param name="factory">
        /// Produces the next result from the contained ok value.
        /// </param>
        /// <returns>
        /// The result <paramref name="factory" /> produced, or the original error
        /// re-wrapped as an <see cref="Err{TOk,TErr}" /> of
        /// <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Result<TOut, TErr>> AndThenAsync<TOut>(
            Func<TOk, ValueTask<Result<TOut, TErr>>> factory)
            where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return await result.AndThenAsync(factory).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> receiver, then calls the
        /// synchronous <paramref name="factory" /> with the contained value if the
        /// result is an <see cref="Ok{TOk,TErr}" />, otherwise propagates the error.
        /// </summary>
        /// <remarks>
        /// The receiver is awaited once, so it must not have been awaited already.
        /// <paramref name="factory" /> is not invoked for an
        /// <see cref="Err{TOk,TErr}" />.
        /// </remarks>
        /// <typeparam name="TOut">
        /// The ok value type of the result produced by <paramref name="factory" />.
        /// </typeparam>
        /// <param name="factory">
        /// Produces the next result from the contained ok value.
        /// </param>
        /// <returns>
        /// The result <paramref name="factory" /> produced, or the original error
        /// re-wrapped as an <see cref="Err{TOk,TErr}" /> of
        /// <typeparamref name="TOut" />.
        /// </returns>
        public async ValueTask<Result<TOut, TErr>> AndThenAsync<TOut>(
            Func<TOk, Result<TOut, TErr>> factory) where TOut : notnull
        {
            Result<TOk, TErr> result = await resultTask.ConfigureAwait(false);

            return result.AndThen(factory);
        }
    }
}
