namespace Waystone.Monads.Configuration.Sample;

using Monads.Diagnostics;

internal sealed class ForgottenInstallWatcher : IDisposable
{
    private readonly IDisposable _subscription;

    public ForgottenInstallWatcher()
    {
        _subscription =
            MonadDiagnostics.ConfigurationNotAppliedEvent.Subscribe(
                _ => Seen++);
    }

    public int Seen { get; private set; }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}
