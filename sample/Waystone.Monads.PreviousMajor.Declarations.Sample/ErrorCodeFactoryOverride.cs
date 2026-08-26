namespace Waystone.Monads.PreviousMajor.Declarations.Sample;

using System;
using Waystone.Monads.Configs;
using Waystone.Monads.Results.Errors;

/// <summary>
/// A consumer subclass overriding the virtual that <c>DRA-129</c> removes. The
/// <c>override</c> keyword makes this a declaration-phase break — <c>CS0115</c>
/// rather than the <c>CS1061</c> a call site gets — which is why it lives here
/// rather than beside the calls.
/// </summary>
internal sealed class UpperCaseErrorCodeFactory : ErrorCodeFactory
{
    public override ErrorCode FromEnum(Enum @enum) =>
        new ErrorCode(@enum.ToString().ToUpperInvariant());
}
