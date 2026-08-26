namespace Waystone.Monads.PreviousMajor.Sample;

using System;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
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
}
