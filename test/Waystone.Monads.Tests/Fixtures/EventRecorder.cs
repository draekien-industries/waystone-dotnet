namespace Waystone.Monads.Fixtures;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Diagnostics;

/// <summary>
/// Subscribes to one <see cref="MonadDiagnostics" /> event the way a consumer
/// does — through <see cref="DiagnosticListener.AllListeners" /> rather than
/// through the internal listener field — and records its payloads.
/// </summary>
/// <remarks>
/// The listener is process-wide, so a test running in another collection can
/// write to the same event. Pass a predicate wherever the payload carries
/// something identifying this test's own work, or those land here too.
/// </remarks>
public sealed class EventRecorder<TPayload> : IDisposable
    where TPayload : class
{
    private readonly IDisposable _allListeners;
    private readonly string _eventName;
    private readonly ConcurrentQueue<TPayload> _events = new();
    private readonly Func<TPayload, bool>? _keep;

    private IDisposable? _subscription;

    /// <param name="eventName">
    /// The <see cref="MonadDiagnostics" /> event name to subscribe to. Nothing
    /// else is subscribed, so the listener stays disabled for every other event
    /// this recorder does not name.
    /// </param>
    /// <param name="keep">
    /// A predicate deciding which payloads to record. Records every payload of
    /// <typeparamref name="TPayload" /> when null.
    /// </param>
    public EventRecorder(string eventName, Func<TPayload, bool>? keep = null)
    {
        _eventName = eventName;
        _keep = keep;
        _allListeners = DiagnosticListener.AllListeners.Subscribe(
            new Observer<DiagnosticListener>(Attach));
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        _allListeners.Dispose();
    }

    public IReadOnlyList<TPayload> Recorded() => _events.ToList();

    private void Attach(DiagnosticListener listener)
    {
        if (listener.Name != MonadDiagnostics.ListenerName)
        {
            return;
        }

        _subscription = listener.Subscribe(
            new Observer<KeyValuePair<string, object?>>(Record),
            name => name == _eventName);
    }

    private void Record(KeyValuePair<string, object?> written)
    {
        if (written.Value is TPayload payload
         && (_keep is null || _keep(payload)))
        {
            _events.Enqueue(payload);
        }
    }
}
