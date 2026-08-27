namespace Waystone.Monads.Extensions.Logging.Configs;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Diagnostics;
using Microsoft.Extensions.Logging;

internal static class ExceptionHandledLogger
{
    private const string MessageTemplate =
        "Waystone.Monads handled an exception thrown by {ArgumentExpression} in {MemberName} at line {LineNumber}.";

    private static readonly Lazy<IDisposable> AllListeners = new(
        () => DiagnosticListener.AllListeners.Subscribe(
            new Observer<DiagnosticListener>(Attach)));

    internal static void Subscribe()
    {
        _ = AllListeners.Value;
    }

    private static void Attach(DiagnosticListener listener)
    {
        if (listener.Name != MonadDiagnostics.ListenerName)
        {
            return;
        }

        listener.Subscribe(
            new Observer<KeyValuePair<string, object?>>(Write),
            static name => name == MonadDiagnostics.ExceptionHandledEventName);
    }

    private static void Write(KeyValuePair<string, object?> written)
    {
        if (written.Value is not ExceptionHandled handled)
        {
            return;
        }

        MonadLoggingOptions options = MonadLoggingOptions.Current;

        if (!options.Logger.IsEnabled(options.Level))
        {
            return;
        }

        options.Logger.Log(
            options.Level,
            handled.Exception,
            MessageTemplate,
            handled.Caller.ArgumentExpression,
            handled.Caller.MemberName,
            handled.Caller.LineNumber);
    }

    [ExcludeFromCodeCoverage]
    private sealed class Observer<T>(Action<T> onNext) : IObserver<T>
    {
        public void OnCompleted()
        { }

        public void OnError(Exception error)
        { }

        public void OnNext(T value)
        {
            onNext(value);
        }
    }
}
