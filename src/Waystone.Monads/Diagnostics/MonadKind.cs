namespace Waystone.Monads.Diagnostics;

/// <summary>Identifies which monad handled an exception.</summary>
/// <remarks>
/// Reported alongside every handled exception because the two cases lose
/// different amounts of information. An exception caught by
/// <c>Option.Try</c> is discarded and survives only in this signal, whereas one
/// caught by <c>Result.Try</c> is also converted into the resulting
/// <c>Err</c> and is still available to the caller.
/// </remarks>
public enum MonadKind
{
    /// <summary>The exception was handled by <c>Option.Try</c> or <c>Option.TryAsync</c>.</summary>
    Option = 0,

    /// <summary>The exception was handled by <c>Result.Try</c> or <c>Result.TryAsync</c>.</summary>
    Result = 1,
}
