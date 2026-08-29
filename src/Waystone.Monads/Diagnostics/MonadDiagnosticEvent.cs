namespace Waystone.Monads.Diagnostics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>One of the events <see cref="MonadDiagnostics" /> writes, paired with the type of payload it carries.</summary>
/// <typeparam name="TPayload">The type written as the event's payload.</typeparam>
/// <remarks>
/// Subscribing through the raw <see cref="DiagnosticListener" /> API means naming
/// the listener and the event as strings and casting the payload out of an
/// <see cref="object" />. Getting any of the three wrong fails silently — no
/// exception, no warning, an empty dashboard. This type carries all three
/// together, so <see cref="Subscribe" /> cannot be pointed at the wrong event or
/// handed the wrong payload type.
/// <para>
/// Instances come from <see cref="MonadDiagnostics" /> and nowhere else; the
/// constructor is internal so an event that the library does not write cannot be
/// named. The raw <see cref="DiagnosticListener" /> path keeps working unchanged
/// and still needs no Waystone package — this is a shortcut over the standard
/// API, not a replacement for it.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using IDisposable watching = MonadDiagnostics.ExceptionHandledEvent.Subscribe(
///     handled => Console.WriteLine(handled.Exception));
/// </code>
/// </example>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed class MonadDiagnosticEvent<TPayload> where TPayload : class
{
    internal MonadDiagnosticEvent(string name)
    {
        Name = name;
    }

    /// <summary>Gets the string this event is written under.</summary>
    /// <remarks>
    /// Needed only to subscribe through <see cref="DiagnosticListener" /> by hand,
    /// or to name the event in a log line or a dashboard.
    /// <see cref="Subscribe" /> supplies it for you.
    /// </remarks>
    public string Name { get; }

    /// <summary>Calls <paramref name="onEvent" /> with each payload this event writes, until the returned subscription is disposed.</summary>
    /// <param name="onEvent">
    /// Receives the payload of every write of this event, from anywhere in the
    /// process. Writes carrying anything other than a
    /// <typeparamref name="TPayload" /> are skipped rather than passed as null.
    /// </param>
    /// <returns>The subscription. Disposing it stops the calls; disposing it twice is harmless.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="onEvent" /> is null.</exception>
    /// <remarks>
    /// <paramref name="onEvent" /> runs synchronously on whichever thread wrote the
    /// event — for <see cref="MonadDiagnostics.ExceptionHandledEvent" /> that is the
    /// throwing thread, inside the <c>catch</c> block that swallowed the exception.
    /// Nothing here moves the work off that thread, so a slow callback slows the
    /// code being observed, and a callback touching shared state needs its own
    /// synchronisation.
    /// <para>
    /// An exception thrown by <paramref name="onEvent" /> propagates out of the
    /// write and into the library code that made it, matching the raw
    /// <see cref="DiagnosticListener" /> path. Nothing is swallowed: throwing from
    /// the callback is how you make a diagnostic event fatal in a test suite.
    /// Catch inside <paramref name="onEvent" /> if that is not what you want,
    /// because an exception escaping here can displace the failure the library was
    /// reporting on.
    /// </para>
    /// <para>
    /// Subscribing attaches to <see cref="DiagnosticListener.AllListeners" />, which
    /// is process-wide, and disposing the return value detaches from both that and
    /// the event itself. Abandoning the subscription without disposing it leaks one
    /// observer that lives as long as the process — acceptable for a subscriber
    /// meant to run for the life of the application, a leak anywhere else.
    /// Subscribing more than once delivers each payload once per subscription.
    /// </para>
    /// </remarks>
    public IDisposable Subscribe(Action<TPayload> onEvent)
    {
        if (onEvent is null)
        {
            throw new ArgumentNullException(nameof(onEvent));
        }

        return new Subscription(Name, onEvent);
    }

    private sealed class Subscription : IDisposable,
                                        IObserver<DiagnosticListener>,
                                        IObserver<KeyValuePair<string, object?>>
    {
        private readonly IDisposable _allListeners;
        private readonly string _eventName;
        private readonly List<IDisposable> _events = new();
        private readonly object _gate = new();
        private readonly Predicate<string> _isThisEvent;
        private readonly Action<TPayload> _onEvent;

        private bool _disposed;

        internal Subscription(string eventName, Action<TPayload> onEvent)
        {
            _eventName = eventName;
            _onEvent = onEvent;
            _isThisEvent = name => name == eventName;
            _allListeners = DiagnosticListener.AllListeners.Subscribe(this);
        }

        public void Dispose()
        {
            IDisposable[] events;

            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                events = _events.ToArray();
                _events.Clear();
            }

            foreach (IDisposable subscription in events)
            {
                subscription.Dispose();
            }

            _allListeners.Dispose();
        }

        void IObserver<DiagnosticListener>.OnNext(DiagnosticListener listener)
        {
            if (listener.Name != MonadDiagnostics.ListenerName)
            {
                return;
            }

            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                _events.Add(listener.Subscribe(this, _isThisEvent));
            }
        }

        void IObserver<KeyValuePair<string, object?>>.OnNext(
            KeyValuePair<string, object?> written)
        {
            if (written.Key == _eventName && written.Value is TPayload payload)
            {
                _onEvent(payload);
            }
        }

        void IObserver<DiagnosticListener>.OnCompleted()
        { }

        [ExcludeFromCodeCoverage]
        void IObserver<DiagnosticListener>.OnError(Exception error)
        { }

        void IObserver<KeyValuePair<string, object?>>.OnCompleted()
        { }

        [ExcludeFromCodeCoverage]
        void IObserver<KeyValuePair<string, object?>>.OnError(Exception error)
        { }
    }
}
