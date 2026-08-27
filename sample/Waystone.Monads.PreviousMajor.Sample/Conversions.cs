namespace Waystone.Monads.PreviousMajor.Sample;

using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>
/// The implicit conversions, which <c>DRA-119</c> removes. Every assignment here is
/// a conversion the compiler performs today and would report as <c>CS0029</c>
/// afterwards, so the count in this file is that issue's break inventory.
/// </summary>
internal static class Conversions
{
    internal static Option<int> FromValue() => 5;

    internal static Result<int, Error> FromOk() => 5;

    internal static Result<int, Error> FromErr() =>
        new Error("order.refused", "refused");

    internal static ErrorCode FromString() => "order.not-found";

    internal static string ToStringCode() => new ErrorCode("order.not-found");

    /// <summary>
    /// A conversion in argument position rather than in a return, which binds by a
    /// different rule and is worth counting separately.
    /// </summary>
    internal static Option<int> Passed() => Widen(7);

    private static Option<int> Widen(Option<int> option) => option;
}
