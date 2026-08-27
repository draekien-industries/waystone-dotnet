namespace Waystone.Monads.Diagnostics;

using System.Diagnostics;
using Configs;

/// <summary>
/// The payload of the
/// <see cref="MonadDiagnostics.ScopeDisposedOutOfOrderEventName" /> diagnostic
/// event.
/// </summary>
/// <remarks>
/// Subscribers receive this boxed as <see cref="object" />, since
/// <see cref="DiagnosticListener" /> is untyped; cast to this record to read it.
/// The event reports a bug in the disposing code rather than anything the library
/// did: a <see cref="MonadOptionsScope" /> was disposed while a scope begun after
/// it was still live. The library leaves the live scope alone in that case, so
/// nothing is silently reconfigured — but disposing the live scope later restores
/// <em>its</em> predecessor, which is the options the early-disposed scope
/// installed, so those options outlive the scope that installed them until the
/// flow unwinds.
/// <para>
/// A subscriber runs synchronously inside
/// <see cref="MonadOptionsScope.Dispose" /> on the thread that disposed, so a
/// stack trace captured there names the offending call site — the one thing this
/// payload cannot carry, because <see cref="System.IDisposable.Dispose" /> takes
/// no caller information. Throwing from a subscriber propagates out of the
/// caller's <c>using</c> and can displace an in-flight exception, so record and
/// return.
/// </para>
/// </remarks>
/// <param name="Scope">
/// The options the disposed scope had installed, or null when it was a
/// default-constructed <see cref="MonadOptionsScope" /> that was never begun.
/// </param>
/// <param name="Live">
/// The options left in effect, belonging to whichever scope is still the
/// innermost one. Null when the flow has no scope left at all.
/// </param>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed record ScopeDisposedOutOfOrder(
    MonadOptions? Scope,
    MonadOptions? Live);
