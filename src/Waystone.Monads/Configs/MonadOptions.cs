namespace Waystone.Monads.Configs;

using System;
using System.Diagnostics;
using Waystone.Monads.Options;
using Waystone.Monads.Results.Errors;

/// <summary>
/// Global configuration options for the Waystone.Monads library.
/// </summary>
public class MonadOptions
{
    private static readonly Lazy<MonadOptions> _instance =
        new(() => new MonadOptions());

    internal static MonadOptions Instance => _instance.Value;

    private MonadOptions()
    {
        ExceptionLogger = Option.None<Action<Exception, CallerInfo>>();
        ErrorCodeFactory = new ErrorCodeFactory();
    }

    internal Option<Action<Exception, CallerInfo>> ExceptionLogger { get; set; }
    internal ErrorCodeFactory ErrorCodeFactory { get; set; }

    internal void Log(Exception exception, CallerInfo callerInfo) =>
        ExceptionLogger.Inspect(logger => logger.Invoke(exception, callerInfo));

    /// <summary>
    /// Configures the log action that should be executed when an exception is silently
    /// handled by the library.
    /// </summary>
    /// <param name="action">The log action that will be executed.</param>
    /// <returns>The <see cref="MonadOptions"/> instance for you to chain additional configurations.</returns>
    public static MonadOptions UseExceptionLogger(Action<Exception, CallerInfo> action)
    {
        Instance.ExceptionLogger = Option.Some(action);
        return Instance;
    }

    /// <summary>
    /// Configures the factory that will be used to create <see cref="ErrorCode"/> instances
    /// from enums and exceptions.
    /// </summary>
    /// <param name="factory">The implementation of <see cref="ErrorCodeFactory"/> you want the library to use.</param>
    /// <returns>The <see cref="MonadOptions"/> instance for you to chain additional configurations.</returns>
    public static MonadOptions UseErrorCodeFactory(ErrorCodeFactory factory)
    {
        Instance.ErrorCodeFactory = factory;
        return Instance;
    }
}