namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;

/// <summary>
/// Unzips an <see cref="Option{T}" /> of a tuple that is still inside a
/// <see cref="Task{TResult}" /> or <see cref="ValueTask{TResult}" />.
/// </summary>
public static class UnzipExtensions
{
    extension<T1, T2>(Task<Option<(T1, T2)>> optionTask)
        where T1 : notnull where T2 : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and unzips the option containing a
        /// tuple into a tuple of two options.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing a pair of <see cref="Some{T}" />
        /// options carrying the two halves of the tuple if the awaited option was a
        /// <see cref="Some{T}" />, otherwise a pair of <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<(Option<T1>, Option<T2>)> UnzipAsync()
        {
            Option<(T1, T2)> option =
                await optionTask.ConfigureAwait(false);

            return option.Unzip();
        }
    }

    extension<T1, T2>(ValueTask<Option<(T1, T2)>> optionTask)
        where T1 : notnull where T2 : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and unzips the option containing
        /// a tuple into a tuple of two options.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing a pair of <see cref="Some{T}" />
        /// options carrying the two halves of the tuple if the awaited option was a
        /// <see cref="Some{T}" />, otherwise a pair of <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<(Option<T1>, Option<T2>)> UnzipAsync()
        {
            Option<(T1, T2)> option =
                await optionTask.ConfigureAwait(false);

            return option.Unzip();
        }
    }
}
