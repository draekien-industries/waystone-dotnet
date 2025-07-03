namespace Waystone.Monads.Configs;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Waystone.Monads.Options;
using Waystone.Monads.Results.Errors;

/// <summary>
/// Global configuration options for the Waystone.Monads library.
/// </summary>
[ExcludeFromCodeCoverage]
public class MonadOptions
{
    private static readonly Lazy<MonadOptions> _singleton =
        new(() => new MonadOptions());

    internal static MonadOptions Global => _singleton.Value;

    private MonadOptions()
    {
        ExceptionLogger = Option.None<Action<Exception, CallerInfo>>();
        ErrorCodeFactory = new ErrorCodeFactory();
        FallbackErrorCode = "Unspecified";
        FallbackErrorMessage = "An unexpected error occurred.";
    }

    internal Option<Action<Exception, CallerInfo>> ExceptionLogger { get; set; }
    internal ErrorCodeFactory ErrorCodeFactory { get; set; }
    internal string FallbackErrorCode { get; set; }
    internal string FallbackErrorMessage { get; set; }

    internal void Log(Exception exception, CallerInfo callerInfo)
    {
        if (Debugger.IsAttached)
        {
            Console.WriteLine("[Waystone.Monads] Exception silently handled:");
            Console.WriteLine($"  Message: {exception.Message}");
            Console.WriteLine($"  Type: {exception.GetType().FullName}");
            Console.WriteLine($"  StackTrace: {exception.StackTrace}");
            Console.WriteLine($"  Caller: {callerInfo.MemberName} at line {callerInfo.LineNumber}");
            Console.WriteLine($"  Argument Expression: {callerInfo.ArgumentExpression}");
        }

        ExceptionLogger.Inspect(logger => logger.Invoke(exception, callerInfo));
    }

    /// <summary>
    /// Configures the global options for the Waystone.Monads library.
    /// </summary>
    /// <param name="configure">The action that will configure the <see cref="MonadOptions"/></param>
    public static void Configure(Action<MonadOptions> configure)
    {
        configure.Invoke(Global);
    }

    /// <summary>
    /// Configures the log action that should be executed when an exception is silently
    /// handled by the library.
    /// </summary>
    /// <param name="action">The log action that will be executed.</param>
    /// <returns>The <see cref="MonadOptions"/> instance for you to chain additional configurations.</returns>
    public MonadOptions UseExceptionLogger(Action<Exception, CallerInfo> action)
    {
        ExceptionLogger = Option.Some(action);
        return this;
    }

    /// <summary>
    /// Configures the factory that will be used to create <see cref="ErrorCode"/> instances
    /// from enums and exceptions.
    /// </summary>
    /// <param name="factory">The implementation of <see cref="ErrorCodeFactory"/> you want the library to use.</param>
    /// <returns>The <see cref="MonadOptions"/> instance for you to chain additional configurations.</returns>
    public MonadOptions UseErrorCodeFactory(ErrorCodeFactory factory)
    {
        ErrorCodeFactory = factory;
        return this;
    }

    /// <summary>
    /// Configures the fallback error code that will be used when a null or whitespace value
    /// is used to create an <see cref="ErrorCode"/> instance.
    /// </summary>
    /// <remarks>The default fallback is `Unspecified`</remarks>
    /// <param name="errorCode">The fallback error code to use</param>
    /// <returns>The <see cref="MonadOptions"/> instance for you to chain additional configurations.</returns>
    public MonadOptions UseFallbackErrorCode(string errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            throw new InvalidOperationException("The fallback error code cannot be null or whitespace.");

        FallbackErrorCode = errorCode.Trim();
        return this;
    }

    /// <summary>
    /// Configures the fallback error message that will be used when a null or whitespace message
    /// is used to create an <see cref="Error"/> instance.
    /// </summary>
    /// <remarks>The default fallback is `An unexpected error occurred.`</remarks>
    /// <param name="errorMessage">The fallback error message to use</param>
    /// <returns>The <see cref="MonadOptions"/> instance for you to chain additional configurations.</returns>
    public MonadOptions UseFallbackErrorMessage(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new InvalidOperationException("The fallback error message cannot be null or whitespace.");

        FallbackErrorMessage = errorMessage.Trim();
        return this;
    }
}