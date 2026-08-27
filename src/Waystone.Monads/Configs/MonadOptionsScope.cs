namespace Waystone.Monads.Configs;

using System;
using Diagnostics;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>
/// Represents an active <see cref="MonadOptions" /> override for the current
/// asynchronous flow.
/// </summary>
/// <remarks>
/// Dispose the scope to restore the options that were in effect before it
/// was created. Created by
/// <see cref="MonadOptions.BeginScope(System.Action{MonadOptions})" />.
/// </remarks>
#if !DEBUG
[DebuggerStepThrough]
#endif
public readonly struct MonadOptionsScope : IDisposable
{
    private readonly MonadOptions? _previous;
    private readonly MonadOptions? _scope;

    internal MonadOptionsScope(MonadOptions? previous, MonadOptions scope)
    {
        _previous = previous;
        _scope = scope;
    }

    /// <summary>Restores the options that were in effect before this scope.</summary>
    /// <remarks>
    /// Restores only while this scope is the innermost live one. Disposing an outer
    /// scope while an inner one is still live leaves the inner scope alone and
    /// writes the
    /// <see cref="MonadDiagnostics.ScopeDisposedOutOfOrderEventName" /> event, as
    /// does disposing a default-constructed scope, which has nothing to restore.
    /// Neither throws: a scope is disposed from a <c>using</c>, where an exception
    /// would displace whichever one was already unwinding.
    /// <para>
    /// Two consequences of declining rather than throwing. The options the
    /// out-of-order scope installed stay in effect until the live scope is
    /// disposed, which then restores them as its own predecessor — so they outlive
    /// the scope that installed them, and only the diagnostic event says so.
    /// And an unobserved process is left with a flow whose options are wrong for a
    /// reason nothing reported, which is why the event is worth subscribing to in a
    /// test suite even if never in production.
    /// </para>
    /// <para>
    /// Safe to call more than once. A second call finds the options it already
    /// restored and returns without writing the event, so an explicit
    /// <c>Dispose</c> inside a <c>using</c> is not reported as misuse.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        MonadOptions? live = MonadOptions.ScopedOptions.Value;

        if (ReferenceEquals(live, _scope))
        {
            MonadOptions.ScopedOptions.Value = _previous;
            return;
        }

        if (ReferenceEquals(live, _previous))
        {
            return;
        }

        MonadDiagnostics.RecordScopeDisposedOutOfOrder(_scope, live);
    }
}
