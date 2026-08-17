namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Exceptions;

public static class ExpectExtensions
{
    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Asynchronously awaits the <see cref="Option{T}" /> then returns the
        /// contained <see cref="Some{T}" /> value.
        /// </summary>
        /// <param name="message">
        /// The message that will be included in the thrown
        /// exception when the option is none.
        /// </param>
        /// <exception cref="UnmetExpectationException">
        /// Thrown when the awaited
        /// option is a <see cref="None{T}" />
        /// </exception>
        public async Task<T> ExpectAsync(string message)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.Expect(message);
        }
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Asynchronously awaits the <see cref="Option{T}" /> then returns the
        /// contained <see cref="Some{T}" /> value.
        /// </summary>
        /// <param name="message">
        /// The message that will be included in the thrown
        /// exception when the option is none.
        /// </param>
        /// <exception cref="UnmetExpectationException">
        /// Thrown when the awaited
        /// option is a <see cref="None{T}" />
        /// </exception>
        public async Task<T> ExpectAsync(string message)
        {
            Option<T> option = await optionTask.ConfigureAwait(false);

            return option.Expect(message);
        }
    }
}
