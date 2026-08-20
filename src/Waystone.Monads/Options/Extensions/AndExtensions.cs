namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;

public static class AndExtensions
{
    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and returns <see cref="None{T}" /> if
        /// the option is a <see cref="None{T}" />, otherwise returns
        /// <paramref name="other" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the value contained in the other option.</typeparam>
        /// <param name="other">
        /// The option to return when the awaited option is a
        /// <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> of <see cref="Option{TOut}" /> containing
        /// <paramref name="other" /> if the awaited option was a <see cref="Some{T}" />,
        /// or <see cref="None{T}" /> otherwise.
        /// </returns>
        public async ValueTask<Option<TOut>> AndAsync<TOut>(Option<TOut> other)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.And(other);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and returns
        /// <see cref="None{T}" /> if the option is a <see cref="None{T}" />, otherwise
        /// returns <paramref name="other" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the value contained in the other option.</typeparam>
        /// <param name="other">
        /// The option to return when the awaited option is a
        /// <see cref="Some{T}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> of <see cref="Option{TOut}" /> containing
        /// <paramref name="other" /> if the awaited option was a <see cref="Some{T}" />,
        /// or <see cref="None{T}" /> otherwise.
        /// </returns>
        public async ValueTask<Option<TOut>> AndAsync<TOut>(Option<TOut> other)
            where TOut : notnull
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.And(other);
        }
    }
}
