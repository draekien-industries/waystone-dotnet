namespace Waystone.Monads.Diagnostics;

using System;
using System.Diagnostics;
using Configs;

/// <summary>
/// The payload of the
/// <see cref="MonadDiagnostics.ExceptionHandledEventName" /> diagnostic event.
/// </summary>
/// <remarks>
/// Subscribers receive this boxed as <see cref="object" />, since
/// <see cref="DiagnosticListener" /> is untyped; cast to this record to read it.
/// The event fires only for an exception the library swallowed, never for one it
/// let propagate. It is the sole hook for observing one:
/// <c>Waystone.Monads.Extensions.Logging</c> is a subscriber like any other, so
/// installing it displaces nothing and adds nothing you cannot reach here.
/// <para>
/// A subscriber runs synchronously on the thread that threw, still inside the
/// <c>catch</c>, and before the caller receives its <c>None</c> or <c>Err</c>.
/// Slow work in a subscriber delays that caller, and an exception thrown from one
/// propagates out of the <c>Try</c> that was meant to swallow the original. Hand
/// off anything expensive.
/// </para>
/// </remarks>
/// <param name="Exception">
/// The exception as it was thrown, before any conversion into an <c>Error</c>.
/// </param>
/// <param name="Caller">The call site whose delegate threw.</param>
/// <param name="Monad">Whether an <c>Option</c> or a <c>Result</c> handled it.</param>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed record ExceptionHandled(
    Exception Exception,
    CallerInfo Caller,
    MonadKind Monad);
