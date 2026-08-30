namespace Waystone.Monads.Extensions.Logging.Configs;

using System;
using Diagnostics;
using Microsoft.Extensions.Logging;

internal static class ExceptionHandledLogger
{
    private const string MessageTemplate =
        "Waystone.Monads handled an exception thrown by {ArgumentExpression} in {MemberName} at line {LineNumber}.";

    private static readonly Lazy<IDisposable> Subscription = new(
        () => MonadDiagnostics.ExceptionHandledEvent.Subscribe(Write));

    internal static void Subscribe()
    {
        _ = Subscription.Value;
    }

    private static void Write(ExceptionHandled handled)
    {
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
}
