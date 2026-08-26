namespace Waystone.Monads.Observability.Sample;

using System.Diagnostics;
using Monads.Diagnostics;

internal sealed class RawEventWatcher : IObserver<DiagnosticListener>
{
    public void OnNext(DiagnosticListener listener)
    {
        if (listener.Name != MonadDiagnostics.ListenerName)
        {
            return;
        }

        listener.Subscribe(
            new HandledExceptions(),
            static name => name == MonadDiagnostics.ExceptionHandledEventName);
    }

    public void OnCompleted()
    { }

    public void OnError(Exception error)
    { }

    private sealed class HandledExceptions
        : IObserver<KeyValuePair<string, object?>>
    {
        public void OnNext(KeyValuePair<string, object?> written)
        {
            if (written.Value is not ExceptionHandled handled)
            {
                return;
            }

            Console.WriteLine(
                $"  raw event: {handled.Monad} at {handled.Caller.MemberName}:"
              + $"{handled.Caller.LineNumber} caught "
              + $"{handled.Exception.GetType().Name}");
        }

        public void OnCompleted()
        { }

        public void OnError(Exception error)
        { }
    }
}
