namespace Waystone.Monads.Configs;

using System;
using System.Diagnostics.CodeAnalysis;
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
[ExcludeFromCodeCoverage]
#if !DEBUG
[DebuggerStepThrough]
#endif
public readonly struct MonadOptionsScope : IDisposable
{
    private readonly MonadOptions? _previous;

    internal MonadOptionsScope(MonadOptions? previous)
    {
        _previous = previous;
    }

    /// <summary>Restores the options that were in effect before this scope.</summary>
    /// <remarks>
    /// Safe to call more than once: each call assigns the same saved options back.
    /// Dispose scopes in the reverse of the order you created them — disposing an
    /// outer scope while an inner one is still live restores the outer scope's
    /// predecessor and silently discards the inner. Disposing a default-constructed
    /// scope clears the scoped options, falling the flow back to the globally
    /// configured ones.
    /// </remarks>
    public void Dispose()
    {
        MonadOptions.ScopedOptions.Value = _previous;
    }
}
