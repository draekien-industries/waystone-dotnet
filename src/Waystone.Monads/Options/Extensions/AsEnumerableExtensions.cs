namespace Waystone.Monads.Options.Extensions;

using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsEnumerableExtensions
{
    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="Task{TResult}" /> and returns a sequence over the
        /// possibly contained value.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing a sequence that yields the
        /// contained value once if the awaited option was a <see cref="Some{T}" />,
        /// otherwise an empty sequence.
        /// </returns>
        public async ValueTask<IEnumerable<T>> AsEnumerableAsync()
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.AsEnumerable();
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Awaits the <see cref="ValueTask{TResult}" /> and returns a sequence over the
        /// possibly contained value.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing a sequence that yields the
        /// contained value once if the awaited option was a <see cref="Some{T}" />,
        /// otherwise an empty sequence.
        /// </returns>
        public async ValueTask<IEnumerable<T>> AsEnumerableAsync()
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.AsEnumerable();
        }
    }
}
