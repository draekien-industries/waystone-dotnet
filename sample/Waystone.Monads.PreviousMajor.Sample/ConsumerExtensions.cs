namespace Waystone.Monads.PreviousMajor.Sample;

using System;
using System.Threading.Tasks;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results.Errors;

/// <summary>
/// A consumer's own extensions, declared with the names <c>DRA-120</c> and
/// <c>DRA-121</c> are about to add. Adding an extension method is only
/// non-breaking while nobody already has that name in scope, and <c>Select</c> and
/// <c>Where</c> are the two most likely to be there already. Neither issue carries
/// a break inventory, so this file exists to stop that number being zero by
/// assumption: once ours arrive, the call sites in
/// <see cref="ConsumerExtensionCallers" /> should report <c>CS0121</c>.
/// </summary>
internal static class ConsumerExtensions
{
    internal static Option<TOut> Select<T, TOut>(
        this Option<T> option,
        Func<T, TOut> selector)
        where T : notnull where TOut : notnull =>
        option.Map(selector);

    internal static Option<TOut> SelectMany<T, TOut>(
        this Option<T> option,
        Func<T, Option<TOut>> selector)
        where T : notnull where TOut : notnull =>
        option.AndThen(selector);

    internal static Option<T> Where<T>(
        this Option<T> option,
        Func<T, bool> predicate)
        where T : notnull =>
        option.Filter(predicate);

    internal static void Deconstruct<T>(
        this Option<T> option,
        out bool isSome,
        out T? value)
        where T : notnull
    {
        isSome = option.IsSome;
        value = option.UnwrapOrDefault();
    }

    internal static bool TryUnwrap<TOk, TErr>(
        this Result<TOk, TErr> result,
        out TOk? value)
        where TOk : notnull where TErr : notnull
    {
        value = result.UnwrapOrDefault();
        return result.IsOk;
    }

    /// <summary>
    /// The awaited-receiver name a consumer is most likely to have written for
    /// themselves, since <c>Result</c> shipped <c>GetOk</c> for six majors with no
    /// awaited shape beside it. DRA-136 added ours beside it, and this measures
    /// what that does to a consumer who got there first.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>It reports nothing, and that is the result.</b> Extension lookup walks
    /// namespace scopes innermost outward and stops at the first that yields a
    /// candidate, so this declaration — in the caller's own namespace — is found
    /// before <c>Waystone.Monads.Results.Extensions</c>, which arrives by using
    /// directive. Adding an awaited receiver therefore does not collide with a
    /// consumer's own extension of the same name; it is shadowed by it.
    /// </para>
    /// <para>
    /// The return type is the control, not an oversight. Ours returns
    /// <c>ValueTask</c> and this returns <c>Task</c>, so
    /// <see cref="ConsumerExtensionCallers.Succeeded" /> assigning to
    /// <c>Task</c> would report <c>CS0029</c> had it bound to ours. It does not,
    /// which is how the inventory shows which of the two won rather than only that
    /// nothing broke.
    /// </para>
    /// <para>
    /// One probe rather than seven: the outcome follows from the name being taken
    /// and not from which member took it. Note that it also undercuts the
    /// expectation recorded above for <c>Select</c> and <c>Where</c> — those are
    /// declared here in the same position and should be shadowed the same way, so
    /// DRA-120 and DRA-121 should re-measure rather than assume <c>CS0121</c>.
    /// </para>
    /// </remarks>
    internal static async Task<Option<TOk>> GetOkAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> resultTask)
        where TOk : notnull where TErr : notnull =>
        (await resultTask.ConfigureAwait(false)).GetOk();
}

/// <summary>
/// The call sites, kept separate so a collision is reported against a line that
/// reads as a consumer's use rather than as a declaration.
/// </summary>
internal static class ConsumerExtensionCallers
{
    internal static Option<int> Query(Option<int> option) =>
        option.Where(value => value > 0).Select(value => value + 1);

    internal static Option<int> Bind(Option<int> option) =>
        option.SelectMany(value => Option.Some(value + 1));

    internal static string Destructure(Option<int> option)
    {
        (bool isSome, int value) = option;
        return isSome ? value.ToString() : "none";
    }

    internal static int Unwrapped(Result<int, Error> result) =>
        result.TryUnwrap(out int value) ? value : 0;

    internal static Task<Option<int>> Succeeded(
        Task<Result<int, Error>> resultTask) =>
        resultTask.GetOkAsync();
}
