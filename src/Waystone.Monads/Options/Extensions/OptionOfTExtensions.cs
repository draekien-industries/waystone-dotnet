namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Results;
using static Option;

/// <summary>Extensions for <see cref="Option{T}" /></summary>
public static class OptionOfTExtensions
{
    /// <summary>
    /// Converts an <see cref="Option{T}" /> of a <see cref="Task" /> into a
    /// <see cref="Task{T}" /> of an <see cref="Option{T}" />
    /// </summary>
    /// <param name="optionOfTask">An option of a task</param>
    /// <param name="onError">
    /// Optional. A callback which will be invoked if the
    /// resolution of the <see cref="Task{T}" /> throws an exception. Not providing one
    /// will mean the exception gets swallowed.
    /// </param>
    /// <typeparam name="T">The return value of the task</typeparam>
    /// <returns>A task of an option</returns>
    public static async Task<Option<T>> Awaited<T>(
        this Option<Task<T>> optionOfTask,
        Action<Exception>? onError = null)
        where T : notnull
    {
        try
        {
            if (optionOfTask.IsNone) return None<T>();

            T value = await optionOfTask.Unwrap().ConfigureAwait(false);
            return Some(value);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            return None<T>();
        }
    }

    /// <summary>Unzips an option containing a tuple value into two options.</summary>
    /// <param name="option">The option to be unzipped.</param>
    /// <typeparam name="T1">The first option value's type.</typeparam>
    /// <typeparam name="T2">The second option value's type.</typeparam>
    /// <returns>
    /// If <paramref name="option" /> is <c>Some&lt;(T1, T2)&gt;</c> this
    /// method returns <c>(Some&lt;T1&gt;, Some&lt;T2&gt;)</c>. Otherwise
    /// <c>(None&lt;T1&gt;, None&lt;T2&gt;)</c> is returned.
    /// </returns>
    public static (Option<T1>, Option<T2>) Unzip<T1, T2>(
        this Option<(T1, T2)> option) where T1 : notnull where T2 : notnull =>
        option.Match(
            tuple => (Some(tuple.Item1), Some(tuple.Item2)),
            () => (None<T1>(), None<T2>()));

    /// <summary>
    /// Converts from <c>Option&lt;Option&lt;T&gt;&gt;</c> to
    /// <c>Option&lt;T&gt;</c>.
    /// </summary>
    /// <remarks>Flattening only removes one level of nesting at a time.</remarks>
    /// <param name="option">The option that needs to be flattened.</param>
    /// <typeparam name="T">The option value's type.</typeparam>
    public static Option<T> Flatten<T>(this Option<Option<T>> option)
        where T : notnull =>
        option.Match(innerOption => innerOption, None<T>);

    /// <summary>
    /// Transposes an <see cref="Option{T}" /> of a <see cref="Result" /> into
    /// a <see cref="Result{TOk,TErr}" /> of an <see cref="Option{T}" />.
    /// </summary>
    /// <list type="bullet">
    /// <item>
    /// <see cref="None{T}" /> will be mapped to <see cref="Ok{TOk,TErr}" /> of
    /// <see cref="None{T}" />
    /// </item>
    /// <item>
    /// <see cref="Some{T}" /> of <see cref="Ok{TOk,TErr}" /> and
    /// <see cref="Some{T}" /> of <see cref="None{T}" /> will be mapped to
    /// <see cref="Ok{TOk,TErr}" /> of <see cref="Some{T}" /> and
    /// <see cref="Err{TOk,TErr}" />
    /// </item>
    /// </list>
    /// <param name="option">The option to transpose into a result</param>
    /// <typeparam name="TOk">The ok result value type</typeparam>
    /// <typeparam name="TErr">The error result value type</typeparam>
    public static Result<Option<TOk>, TErr> Transpose<TOk, TErr>(
        this Option<Result<TOk, TErr>> option)
        where TOk : notnull where TErr : notnull =>
        option.Match(
            some => some.Match(
                ok => Result.Ok<Option<TOk>, TErr>(Some(ok)),
                Result.Err<Option<TOk>, TErr>),
            () => Result.Ok<Option<TOk>, TErr>(None<TOk>()));
}
