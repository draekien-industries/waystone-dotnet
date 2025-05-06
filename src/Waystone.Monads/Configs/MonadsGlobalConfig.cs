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
    internal static Option<Action<Exception>> LogAction { get; set; } =
#if DEBUG
        Option.Some<Action<Exception>>(ex => Debug.WriteLine(
                                           $"[Waystone.Monads] {ex}"));
#else
        Option.None<Action<Exception>>();
#endif

    internal static void LogException(
        Exception ex) =>
        LogAction.Inspect(action => action.Invoke(ex));


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
        LogAction = Option.Some(log);
    }
}
