namespace Waystone.Monads.Configs;

using System;
using System.Diagnostics;
using Options;

/// <summary>
/// Provides configuration options and utilities for handling monadic
/// operations in the Waystone.Monads library, including global exception handling
/// mechanisms.
/// </summary>
public static class MonadsGlobalConfig
{
    private static Option<Action<Exception, CallerInfo>>
        LogAction { get; set; } =
#if DEBUG
        Option.Some<Action<Exception, CallerInfo>>((ex, callerInfo) =>
        {
            Debug.WriteLine(
                $"[Waystone.Monads] Handled exception of type '{ex.GetType().Name}':");
            Debug.Indent();
            Debug.WriteLine($"- Caller Member Name: {callerInfo.MemberName}");
            Debug.WriteLine($"- Caller Line Number: {callerInfo.LineNumber}");
            Debug.WriteLine(
                $"- Caller Argument Expression: {callerInfo.ArgumentExpression}");
            Debug.WriteLine("--- Exception Details ---");
            Debug.WriteLine(ex);
            Debug.Unindent();
        });
#else
        Option.None<Action<Exception, CallerInfo>>();
#endif

    internal static void Log(
        Exception ex,
        CallerInfo callerInfo) =>
        LogAction.Inspect(action => action.Invoke(ex, callerInfo));


    /// <summary>
    /// Configures the global exception logger used in the Waystone.Monads
    /// library. Any exceptions occurring during certain operations can be logged using
    /// the provided action.
    /// </summary>
    /// <param name="log">
    /// The action to execute when logging exceptions. Accepts an
    /// <see cref="Exception" /> parameter.
    /// </param>
    public static void UseExceptionLogger(
        Action<Exception> log)
    {
        LogAction =
            Option.Some<Action<Exception, CallerInfo>>((ex, _) => log(ex));
    }

    /// <summary>
    /// Configures the global exception logger used in the Waystone.Monads
    /// library. This allows capturing and logging exceptions thrown during monadic
    /// operations, including information about the caller of the operation.
    /// </summary>
    /// <param name="log">
    /// An action to execute when logging exceptions. The action
    /// receives an <see cref="Exception" /> and a <see cref="CallerInfo" /> object
    /// containing details about the caller that caused the exception to throw.
    /// </param>
    public static void UseExceptionLogger(
        Action<Exception, CallerInfo> log)
    {
        LogAction = Option.Some(log);
    }
}
