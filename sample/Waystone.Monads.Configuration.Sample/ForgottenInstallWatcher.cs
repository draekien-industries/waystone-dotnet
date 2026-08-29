namespace Waystone.Monads.Configuration.Sample;

using System.Diagnostics;
using Monads.Diagnostics;

internal sealed class ForgottenInstallWatcher
    : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object?>>,
      IDisposable
{
    private readonly IDisposable _allListeners;

    private IDisposable? _subscription;

    public ForgottenInstallWatcher()
    {
        _allListeners = DiagnosticListener.AllListeners.Subscribe(this);
    }

    public int Seen { get; private set; }

    public void Dispose()
    {
        _subscription?.Dispose();
        _allListeners.Dispose();
    }

    void IObserver<DiagnosticListener>.OnNext(DiagnosticListener listener)
    {
        if (listener.Name == MonadDiagnostics.ListenerName)
        {
            _subscription = listener.Subscribe(
                this,
                name => name
                     == MonadDiagnostics.ConfigurationNotAppliedEventName);
        }
    }

    void IObserver<KeyValuePair<string, object?>>.OnNext(
        KeyValuePair<string, object?> written)
    {
        if (written.Value is ConfigurationNotApplied)
        {
            Seen++;
        }
    }

    void IObserver<DiagnosticListener>.OnCompleted() { }

    void IObserver<DiagnosticListener>.OnError(Exception error) { }

    void IObserver<KeyValuePair<string, object?>>.OnCompleted() { }

    void IObserver<KeyValuePair<string, object?>>.OnError(Exception error) { }
}
