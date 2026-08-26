namespace Waystone.Monads.PreviousMajor.Sample;

using System;
using Waystone.Monads.Configs;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>
/// The five members obsoleted for removal in <c>7.0.0</c>, called deliberately so
/// <c>DRA-129</c> registers as diagnostics rather than as nothing. The project sets
/// <c>NoWarn=CS0618</c> so these calls do not make the floor build noisy while they
/// wait to break.
/// </summary>
internal static class RunTimeErrorCodes
{
    internal static ErrorCode FromEnum() =>
        ErrorCode.FromEnum(OrderError.NotFound);

    internal static Error ErrorFromEnum() =>
        Error.FromEnum(OrderError.AlreadyShipped, "already shipped");

    internal static Result<int, Error> ErrFromEnum() =>
        Result.Err<int>(OrderError.OutOfStock, "out of stock");

    /// <summary>
    /// The virtual called on the base class rather than on a subclass. The
    /// <c>override</c> that <c>DRA-129</c> also breaks is a declaration-phase
    /// diagnostic, so it lives in <c>Declarations/</c> where it cannot mask this
    /// project's body-phase inventory.
    /// </summary>
    internal static ErrorCode ViaFactory() =>
        new ErrorCodeFactory().FromEnum(OrderError.NotFound);
}
