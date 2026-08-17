namespace Waystone.Monads.Configs;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Options;
using Results.Errors;

/// <summary>Global configuration options for the Waystone.Monads library.</summary>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed class MonadOptions
{
    private static readonly Lazy<MonadOptions> Singleton =
        new(() => new MonadOptions());

    internal static readonly AsyncLocal<MonadOptions?> ScopedOptions = new();

    private static volatile bool _scopingHasBeenUsed;

    private ConcurrentDictionary<Type, IMonadOptionsSatellite>? _satellites;

    private MonadOptions()
    {
        ExceptionLogger = Option.None<Action<Exception, CallerInfo>>();
        ErrorCodeFactory = new ErrorCodeFactory();
        FallbackErrorCode = "Unspecified";
        FallbackErrorMessage = "An unexpected error occurred.";
    }

    private MonadOptions(MonadOptions source)
    {
        ExceptionLogger = source.ExceptionLogger;
        ErrorCodeFactory = source.ErrorCodeFactory;
        FallbackErrorCode = source.FallbackErrorCode;
        FallbackErrorMessage = source.FallbackErrorMessage;

        ConcurrentDictionary<Type, IMonadOptionsSatellite>? satellites =
            source._satellites;

        if (satellites is null)
        {
            return;
        }

        foreach (KeyValuePair<Type, IMonadOptionsSatellite> satellite in
                 satellites)
        {
            Satellites[satellite.Key] = satellite.Value.Clone();
        }
    }

    internal static MonadOptions Global => Singleton.Value;

    internal static MonadOptions Current =>
        _scopingHasBeenUsed ? ScopedOptions.Value ?? Global : Global;

    private ConcurrentDictionary<Type, IMonadOptionsSatellite> Satellites =>
        LazyInitializer.EnsureInitialized(ref _satellites)!;

    internal Option<Action<Exception, CallerInfo>> ExceptionLogger { get; set; }
    internal ErrorCodeFactory ErrorCodeFactory { get; set; }
    internal string FallbackErrorCode { get; set; }
    internal string FallbackErrorMessage { get; set; }

    internal T Satellite<T>(Func<T> create)
        where T : class, IMonadOptionsSatellite
    {
        if (_satellites is { } satellites
         && satellites.TryGetValue(typeof(T), out IMonadOptionsSatellite? found))
        {
            return (T)found;
        }

        return (T)Satellites.GetOrAdd(typeof(T), _ => create());
    }

    internal void Log(Exception exception, CallerInfo callerInfo)
    {
        if (Debugger.IsAttached)
        {
            Console.WriteLine("[Waystone.Monads] Exception silently handled:");
            Console.WriteLine($"  Message: {exception.Message}");
            Console.WriteLine($"  Type: {exception.GetType().FullName}");
            Console.WriteLine($"  StackTrace: {exception.StackTrace}");
            Console.WriteLine(
                $"  Caller: {callerInfo.MemberName} at line {callerInfo.LineNumber}");
            Console.WriteLine(
                $"  Argument Expression: {callerInfo.ArgumentExpression}");
        }

        ExceptionLogger.Inspect(logger => logger.Invoke(exception, callerInfo));
    }

    /// <summary>Configures the global options for the Waystone.Monads library.</summary>
    /// <param name="configure">
    /// The action that will configure the
    /// <see cref="MonadOptions" />
    /// </param>
    public static void Configure(Action<MonadOptions> configure)
    {
        configure.Invoke(Global);
    }

    /// <summary>
    /// Creates a snapshot of the options that are currently in effect, with
    /// the provided configuration applied on top.
    /// </summary>
    /// <remarks>
    /// Options that the <paramref name="configure" /> action does not set are
    /// inherited from the options in effect when this method is called. The
    /// returned instance is detached, so later calls to <see cref="Configure" />
    /// do not affect it.
    /// </remarks>
    /// <param name="configure">
    /// The action that will configure the returned
    /// <see cref="MonadOptions" />
    /// </param>
    /// <returns>The created <see cref="MonadOptions" />.</returns>
    internal static MonadOptions Create(Action<MonadOptions> configure)
    {
        var options = new MonadOptions(Current);
        configure.Invoke(options);
        return options;
    }

    /// <summary>
    /// Overrides the options for the current asynchronous flow until the
    /// returned scope is disposed.
    /// </summary>
    /// <remarks>
    /// The scope applies to work started inside it, including work that
    /// continues after an <c>await</c>. It does not affect work that was already
    /// running when the scope was created, and it does not modify the globally
    /// configured options. Scopes may be nested, and disposing one restores the
    /// scope that surrounded it.
    /// </remarks>
    /// <param name="configure">
    /// The action that will configure the scoped
    /// <see cref="MonadOptions" />
    /// </param>
    /// <returns>
    /// A <see cref="MonadOptionsScope" /> which restores the previous options
    /// when disposed.
    /// </returns>
    public static MonadOptionsScope CreateScope(
        Action<MonadOptions> configure) =>
        CreateScope(Create(configure));

    /// <summary>
    /// Overrides the options for the current asynchronous flow with the
    /// provided options until the returned scope is disposed.
    /// </summary>
    /// <remarks>
    /// The provided <paramref name="options" /> are used as they are rather
    /// than copied, so the caller must pass an instance that nothing else holds.
    /// Use <see cref="Create" /> to build one.
    /// </remarks>
    /// <param name="options">The options to use for the duration of the scope.</param>
    /// <returns>
    /// A <see cref="MonadOptionsScope" /> which restores the previous options
    /// when disposed.
    /// </returns>
    internal static MonadOptionsScope CreateScope(MonadOptions options)
    {
        _scopingHasBeenUsed = true;
        MonadOptions? previous = ScopedOptions.Value;
        ScopedOptions.Value = options;
        return new MonadOptionsScope(previous);
    }

    /// <summary>
    /// Configures the log action that should be executed when an exception is
    /// silently handled by the library.
    /// </summary>
    /// <param name="action">The log action that will be executed.</param>
    /// <returns>
    /// The <see cref="MonadOptions" /> instance for you to chain additional
    /// configurations.
    /// </returns>
    public MonadOptions UseExceptionLogger(Action<Exception, CallerInfo> action)
    {
        ExceptionLogger = Option.Some(action);
        return this;
    }

    /// <summary>
    /// Configures the factory that will be used to create
    /// <see cref="ErrorCode" /> instances from enums and exceptions.
    /// </summary>
    /// <param name="factory">
    /// The implementation of <see cref="ErrorCodeFactory" /> you
    /// want the library to use.
    /// </param>
    /// <returns>
    /// The <see cref="MonadOptions" /> instance for you to chain additional
    /// configurations.
    /// </returns>
    public MonadOptions UseErrorCodeFactory(ErrorCodeFactory factory)
    {
        ErrorCodeFactory = factory;
        return this;
    }

    /// <summary>
    /// Configures the fallback error code that will be used when a null or
    /// whitespace value is used to create an <see cref="ErrorCode" /> instance.
    /// </summary>
    /// <remarks>The default fallback is `Unspecified`</remarks>
    /// <param name="errorCode">The fallback error code to use</param>
    /// <returns>
    /// The <see cref="MonadOptions" /> instance for you to chain additional
    /// configurations.
    /// </returns>
    public MonadOptions UseFallbackErrorCode(string errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            throw new ArgumentException(
                "The fallback error code cannot be null or whitespace.",
                nameof(errorCode));
        }

        FallbackErrorCode = errorCode.Trim();
        return this;
    }

    /// <summary>
    /// Configures the fallback error message that will be used when a null or
    /// whitespace message is used to create an <see cref="Error" /> instance.
    /// </summary>
    /// <remarks>The default fallback is `An unexpected error occurred.`</remarks>
    /// <param name="errorMessage">The fallback error message to use</param>
    /// <returns>
    /// The <see cref="MonadOptions" /> instance for you to chain additional
    /// configurations.
    /// </returns>
    public MonadOptions UseFallbackErrorMessage(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException(
                "The fallback error message cannot be null or whitespace.",
                nameof(errorMessage));
        }

        FallbackErrorMessage = errorMessage.Trim();
        return this;
    }
}
