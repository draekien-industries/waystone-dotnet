namespace Waystone.WideLogEvents;

using System;

public abstract record WideLogEventOutcome(WideLogEventOutcomeType Type)
{
    public static readonly WideLogEventOutcome Indeterminate =
        new IndeterminateWideLogEventOutcome();

    public static readonly WideLogEventOutcome Success =
        new SuccessWideLogEventOutcome();

    public static WideLogEventOutcome Failure(Exception exception) =>
        new FailureWideLogEventOutcome(exception);
}

public sealed record IndeterminateWideLogEventOutcome()
    : WideLogEventOutcome(WideLogEventOutcomeType.Indeterminate);

public sealed record SuccessWideLogEventOutcome() : WideLogEventOutcome(
    WideLogEventOutcomeType.Success);

public sealed record FailureWideLogEventOutcome(Exception Exception)
    : WideLogEventOutcome(WideLogEventOutcomeType.Failure);
