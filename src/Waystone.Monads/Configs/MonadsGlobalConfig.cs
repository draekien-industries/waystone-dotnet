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
    private static Action<Exception, CallerInfo> LogAction =>
        (ex, callerInfo) =>
        {
            Debug.WriteLine(
                $"[Waystone.Monads] Handled exception of type '{ex.GetType().Name}':");
            Debug.Indent();
            Debug.WriteLine($"- Caller Member Name: {callerInfo.MemberName}");
            Debug.WriteLine($"- Caller Line Number: {callerInfo.LineNumber}");
            Debug.WriteLine(
                $"- Caller Argument Expression: {callerInfo.ArgumentExpression}");
            Debug.WriteLine("--- Handled Exception Details ---");
            Debug.WriteLine(ex);
            Debug.WriteLine("--- End of Exception Details ---");
            Debug.Unindent();

            ConfiguredLogAction.Inspect(action =>
                                            action.Invoke(ex, callerInfo));
        };

    private static Option<Action<Exception, CallerInfo>> ConfiguredLogAction
    {
        get;
        set;
    } = Option.None<Action<Exception, CallerInfo>>();

    internal static void Log(
        Exception ex,
        CallerInfo callerInfo) =>
        LogAction.Invoke(ex, callerInfo);


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
        ConfiguredLogAction =
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
        ConfiguredLogAction = Option.Some(log);
    }
}
