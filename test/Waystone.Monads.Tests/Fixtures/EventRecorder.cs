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
/// <para>
/// Stays hand-written rather than calling
/// <see cref="MonadDiagnosticEvent{TPayload}.Subscribe" />: this is the oracle
/// proving the library writes its events at all, and routing it through the
/// helper would let one defect in the helper hide a defect in the emission.
/// </para>
/// <para>
/// It holds every matching listener's subscription rather than the last one.
/// More than one <see cref="DiagnosticListener" /> can carry the monad
/// listener's name — <c>MonadDiagnosticEventTests</c> creates its own — and
/// overwriting the field dropped the reference without disposing it, so the
/// recorder kept receiving but leaked a subscription for the rest of the run.
/// </para>
/// </remarks>
public sealed class EventRecorder<TPayload> : IDisposable
    where TPayload : class
{
    private readonly IDisposable _allListeners;
    private readonly string _eventName;
    private readonly ConcurrentQueue<TPayload> _events = new();
    private readonly object _gate = new();
    private readonly Func<TPayload, bool>? _keep;
    private readonly List<IDisposable> _subscriptions = new();

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
        lock (_gate)
        {
            foreach (IDisposable subscription in _subscriptions)
            {
                subscription.Dispose();
            }

            _subscriptions.Clear();
        }

        _allListeners.Dispose();
    }

    public IReadOnlyList<TPayload> Recorded() => _events.ToList();

    private void Attach(DiagnosticListener listener)
    {
        if (listener.Name != MonadDiagnostics.ListenerName)
        {
            return;
        }

        lock (_gate)
        {
            _subscriptions.Add(
                listener.Subscribe(
                    new Observer<KeyValuePair<string, object?>>(Record),
                    name => name == _eventName));
        }
    }

    private void Record(KeyValuePair<string, object?> written)
    {
        if (written.Key == _eventName
         && written.Value is TPayload payload
         && (_keep is null || _keep(payload)))
        {
            _events.Enqueue(payload);
        }
    }
}
