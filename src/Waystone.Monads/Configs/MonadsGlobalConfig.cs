namespace Waystone.Monads.Configs;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Options;

/// <summary>
/// Provides configuration options and utilities for handling monadic
/// operations in the Waystone.Monads library, including global exception handling
/// mechanisms.
/// </summary>
public static class MonadsGlobalConfig
{
    internal static Option<Action<Exception, string>> LogAction { get; set; } =
#if DEBUG
        Option.Some<Action<Exception, string>>((ex, source) => Debug.WriteLine(
                                                   $"[Waystone.Monads::{source}] {ex}"));
#else
        Option.None<Action<Exception, string>>();
#endif

    internal static void LogException(
        Exception ex,
        [CallerMemberName] string source = "") =>
        LogAction.Inspect(action => action.Invoke(ex, source));


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
        LogAction = Option.Some<Action<Exception, string>>((ex, _) => log(ex));
    }
}
