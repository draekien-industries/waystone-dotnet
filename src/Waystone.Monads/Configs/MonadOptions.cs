namespace Waystone.Monads.Configs;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Diagnostics;
using Results.Errors;

/// <summary>Configuration options for the Waystone.Monads library.</summary>
/// <remarks>
/// Settings apply process-wide once <see cref="Configure" /> has been called.
/// Use <see cref="BeginScope(Action{MonadOptions})" /> to override them for one
/// asynchronous flow instead.
/// </remarks>
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
        ErrorCodeFactory = new ErrorCodeFactory();
        FallbackErrorCode = "Unspecified";
        FallbackErrorMessage = "An unexpected error occurred.";
        CatchesCancellation = false;
    }

    private MonadOptions(MonadOptions source)
    {
        ErrorCodeFactory = source.ErrorCodeFactory;
        FallbackErrorCode = source.FallbackErrorCode;
        FallbackErrorMessage = source.FallbackErrorMessage;
        CatchesCancellation = source.CatchesCancellation;

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

    internal ErrorCodeFactory ErrorCodeFactory { get; set; }
    internal string FallbackErrorCode { get; set; }
    internal string FallbackErrorMessage { get; set; }
    internal bool CatchesCancellation { get; set; }

    internal bool Catches(Exception exception) =>
        CatchesCancellation || exception is not OperationCanceledException;

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

    internal void Log(
        Exception exception,
        CallerInfo callerInfo,
        MonadKind monad)
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

        MonadDiagnostics.RecordExceptionHandled(exception, callerInfo, monad);
    }

    /// <summary>Configures the global options for the Waystone.Monads library.</summary>
    /// <remarks>
    /// The options are a single process-wide instance, so this affects every
    /// caller in the process, including work already in flight. Call it once
    /// during start-up. To change options for one asynchronous flow without
    /// disturbing the rest, use
    /// <see cref="BeginScope(Action{MonadOptions})" /> instead.
    /// </remarks>
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
    public static MonadOptionsScope BeginScope(
        Action<MonadOptions> configure) =>
        BeginScope(Create(configure));

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
    internal static MonadOptionsScope BeginScope(MonadOptions options)
    {
        _scopingHasBeenUsed = true;
        MonadOptions? previous = ScopedOptions.Value;
        ScopedOptions.Value = options;
        return new MonadOptionsScope(previous, options);
    }

    /// <summary>
    /// Configures <c>Try</c> and <c>TryAsync</c> to treat a cancellation as a
    /// failure rather than letting it propagate.
    /// </summary>
    /// <remarks>
    /// By default an <see cref="OperationCanceledException" /> is not caught,
    /// so it leaves <c>Try</c> and <c>TryAsync</c> untouched and is neither
    /// logged nor converted. Call this and a cancellation instead produces a
    /// <see cref="Options.None{T}" /> or an <see cref="Results.Err{TOk,TErr}" />
    /// like any other exception, which is what versions before 6.0.0 did.
    /// Prefer the default: a cancelled operation produced no answer, and
    /// reporting that as an absent or failed value hides the cancellation from
    /// the caller that requested it.
    /// <see cref="System.Threading.Tasks.TaskCanceledException" /> derives from
    /// <see cref="OperationCanceledException" /> and is covered by this option
    /// too.
    /// </remarks>
    /// <returns>
    /// The <see cref="MonadOptions" /> instance for you to chain additional
    /// configurations.
    /// </returns>
    public MonadOptions UseCancellationAsFailure()
    {
        CatchesCancellation = true;
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
    /// <remarks>
    /// Default: <c>Unspecified</c>. Surrounding whitespace is trimmed off
    /// before the value is stored.
    /// </remarks>
    /// <param name="errorCode">The fallback error code to use</param>
    /// <returns>
    /// The <see cref="MonadOptions" /> instance for you to chain additional
    /// configurations.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="errorCode" /> is null, empty or whitespace. A fallback
    /// that is itself unusable would leave nothing to fall back to.
    /// </exception>
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
    /// <remarks>
    /// Default: <c>An unexpected error occurred.</c> Surrounding whitespace is
    /// trimmed off before the value is stored.
    /// </remarks>
    /// <param name="errorMessage">The fallback error message to use</param>
    /// <returns>
    /// The <see cref="MonadOptions" /> instance for you to chain additional
    /// configurations.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="errorMessage" /> is null, empty or whitespace. A
    /// fallback that is itself unusable would leave nothing to fall back to.
    /// </exception>
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
