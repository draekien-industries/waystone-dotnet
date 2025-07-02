namespace Waystone.Monads.Configs;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Provides configuration options and utilities for handling monadic
/// operations in the Waystone.Monads library, including global exception handling
/// mechanisms.
/// </summary>
[ExcludeFromCodeCoverage]
[Obsolete("Use `MonadOptions` instead to configure global library behaviour. This class will be removed in a future version.")]
public static class MonadsGlobalConfig
{
    /// <summary>
    /// Configures the global exception logger used in the Waystone.Monads
    /// library. Any exceptions occurring during certain operations can be logged using
    /// the provided action.
    /// </summary>
    /// <param name="log">
    /// The action to execute when logging exceptions. Accepts an
    /// <see cref="Exception" /> parameter.
    /// </param>
    [Obsolete("Use `MonadOptions` instead to configure global library behaviour. This class will be removed in a future version.")]
    public static void UseExceptionLogger(
        Action<Exception> log)
    {
        MonadOptions.Global.UseExceptionLogger((ex, _) => log(ex));
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
    [Obsolete("Use `MonadOptions` instead to configure global library behaviour. This class will be removed in a future version.")]
    public static void UseExceptionLogger(
        Action<Exception, CallerInfo> log)
    {
        MonadOptions.Global.UseExceptionLogger(log);
    }
}
