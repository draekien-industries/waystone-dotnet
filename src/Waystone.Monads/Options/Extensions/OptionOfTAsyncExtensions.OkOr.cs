namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Waystone.Monads.Results;

public static partial class OptionOfTAsyncExtensions
{

    /// <summary>
    /// Transforms the current <see cref="Option{T}"/> into a <see cref="Result{TOk, TErr}"/>, mapping <see cref="Some{T}"/>
    /// to <see cref="Ok{TOk, TErr}"/> and <see cref="None{T}"/> to <see cref="Err{TOk, TErr}"/>.
    /// </summary>
    /// <remarks>
    /// Arguments passed to this method must be eagerly evauated. If you are passing the result of a function call,
    /// it is recommended to use OkOrElseAsync , which is lazily evaluated.
    /// </remarks>
    /// <typeparam name="TOk">The type of the value contained inside the option.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <param name="optionTask">A task which when completed will return an option.</param>
    /// <param name="error">The error.</param>
    /// <returns>
    /// An <see cref="Ok{TOk, TErr}"/> if the current option is a <see cref="Some{T}"/>, otherwise an <see cref="Err{TOk, TErr}"/>.
    /// </returns>
    public static async Task<Result<TOk, TErr>> OkOrAsync<TOk, TErr>(this Task<Option<TOk>> optionTask, TErr error)
          where TOk : notnull
          where TErr : notnull
    {
        Option<TOk> option = await optionTask.ConfigureAwait(false);
        return option.OkOr(error);
    }

    /// <summary>
    /// Transforms the current <see cref="Option{T}"/> into a <see cref="Result{TOk, TErr}"/>, mapping <see cref="Some{T}"/>
    /// to <see cref="Ok{TOk, TErr}"/> and <see cref="None{T}"/> to <see cref="Err{TOk, TErr}"/>.
    /// </summary>
    /// <remarks>
    /// Arguments passed to this method must be eagerly evauated. If you are passing the result of a function call,
    /// it is recommended to use OkOrElseAsync , which is lazily evaluated.
    /// </remarks>
    /// <typeparam name="TOk">The type of the value contained inside the option.</typeparam>
    /// <typeparam name="TErr">The type of the error value.</typeparam>
    /// <param name="optionTask">A value task which when completed will return an option.</param>
    /// <param name="error">The error.</param>
    /// <returns>
    /// An <see cref="Ok{TOk, TErr}"/> if the current option is a <see cref="Some{T}"/>, otherwise an <see cref="Err{TOk, TErr}"/>.
    /// </returns>
    public static async ValueTask<Result<TOk, TErr>> OkOrAsync<TOk, TErr>(this ValueTask<Option<TOk>> optionTask, TErr error)
        where TOk : notnull
        where TErr : notnull
    {
        Option<TOk> option = await optionTask.ConfigureAwait(false);
        return option.OkOr(error);
    }
}