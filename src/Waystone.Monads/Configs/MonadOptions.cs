namespace Waystone.Monads.Configs;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Diagnostics;

/// <summary>Configuration options for the Waystone.Monads library.</summary>
/// <remarks>
/// An instance is immutable, and one instance is in effect at a time. Settings
/// apply process-wide once <see cref="Configure" /> has been called. Use
/// <see cref="BeginScope(Action{MonadOptionsBuilder})" /> to override them for one asynchronous flow instead.
/// <para>
/// Because a snapshot is published whole rather than modified in place, a reader
/// racing a <see cref="Configure" /> call sees either every old setting or every
/// new one, never a mixture.
/// </para>
/// </remarks>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed class MonadOptions
{
    private static readonly object ConfigureGate = new();

    private static readonly MonadOptions Default =
        new MonadOptionsBuilder().Build();

    internal static readonly AsyncLocal<MonadOptions?> ScopedOptions = new();

    private static MonadOptions _global = Default;

    private static volatile bool _scopingHasBeenUsed;

    private static volatile bool _configurationIsPending;

    internal MonadOptions(
        ErrorCodeFactory errorCodeFactory,
        string fallbackErrorCode,
        string fallbackErrorMessage,
        bool catchesCancellation,
        object?[] satellites)
    {
        ErrorCodeFactory = errorCodeFactory;
        FallbackErrorCode = fallbackErrorCode;
        FallbackErrorMessage = fallbackErrorMessage;
        CatchesCancellation = catchesCancellation;
        Satellites = satellites;
    }

    internal static MonadOptions Global => Volatile.Read(ref _global);

    internal static MonadOptions Current
    {
        get
        {
            if (_configurationIsPending)
            {
                ReportConfigurationNotApplied();
            }

            return _scopingHasBeenUsed ? ScopedOptions.Value ?? Global : Global;
        }
    }

    internal ErrorCodeFactory ErrorCodeFactory { get; }
    internal string FallbackErrorCode { get; }
    internal string FallbackErrorMessage { get; }
    internal bool CatchesCancellation { get; }

    internal object?[] Satellites { get; }

    internal bool Catches(Exception exception) =>
        CatchesCancellation || exception is not OperationCanceledException;

    internal T? Satellite<T>(int slot)
        where T : class =>
        MonadOptionsSlot.At<T>(Satellites, slot);

    internal MonadOptionsBuilder ToBuilder() => new(this);

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
    /// The builder arrives carrying the settings already in effect, so this
    /// accumulates rather than starting over: call it twice, changing one setting
    /// each time, and both changes are in force. The snapshot it produces replaces
    /// the previous one for every caller in the process, including work already in
    /// flight, so call it once during start-up. To change options for one
    /// asynchronous flow without disturbing the rest, use
    /// <see cref="BeginScope(Action{MonadOptionsBuilder})" /> instead.
    /// <para>
    /// Concurrent calls are serialised against each other, so neither loses its
    /// changes. Reads are never blocked by one.
    /// </para>
    /// </remarks>
    /// <param name="configure">
    /// The action that will configure the
    /// <see cref="MonadOptionsBuilder" />
    /// </param>
    public static void Configure(Action<MonadOptionsBuilder> configure)
    {
        lock (ConfigureGate)
        {
            MonadOptionsBuilder builder = Global.ToBuilder();
            configure.Invoke(builder);
            _configurationIsPending = false;
            Volatile.Write(ref _global, builder.Build());
        }
    }

    /// <summary>
    /// Overrides the options for the current asynchronous flow until the
    /// returned scope is disposed.
    /// </summary>
    /// <remarks>
    /// The scope applies to work started inside it, including work that
    /// continues after an <c>await</c>. It does not affect work that was already
    /// running when the scope was created, and it does not modify the globally
    /// configured options. Settings the action does not touch are inherited from
    /// the options in effect when the scope opens. Scopes may be nested, and
    /// disposing one restores the scope that surrounded it.
    /// <para>
    /// The first scope opened in a process moves every later options read onto a
    /// path that consults an <see cref="AsyncLocal{T}" />, and nothing moves it
    /// back. Prefer <see cref="Configure" /> where a single process-wide setting
    /// will do.
    /// </para>
    /// </remarks>
    /// <param name="configure">
    /// The action that will configure the scoped
    /// <see cref="MonadOptionsBuilder" />
    /// </param>
    /// <returns>
    /// A <see cref="MonadOptionsScope" /> which restores the previous options
    /// when disposed.
    /// </returns>
    public static MonadOptionsScope BeginScope(
        Action<MonadOptionsBuilder> configure) =>
        BeginScope(Create(configure));

    internal static MonadOptions Create(Action<MonadOptionsBuilder> configure)
    {
        MonadOptionsBuilder builder = Current.ToBuilder();
        configure.Invoke(builder);
        return builder.Build();
    }

    internal static MonadOptionsScope BeginScope(MonadOptions options)
    {
        _scopingHasBeenUsed = true;
        MonadOptions? previous = ScopedOptions.Value;
        ScopedOptions.Value = options;
        return new MonadOptionsScope(previous, options);
    }

    internal static void Install(MonadOptions options)
    {
        lock (ConfigureGate)
        {
            _configurationIsPending = false;
            Volatile.Write(ref _global, options);
        }
    }

    internal static void MarkConfigurationPending()
    {
        _configurationIsPending = true;
    }

    internal static void Reset()
    {
        lock (ConfigureGate)
        {
            _configurationIsPending = false;
            Volatile.Write(ref _global, Default);
            ScopedOptions.Value = null;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ReportConfigurationNotApplied()
    {
        if (MonadDiagnostics.RecordConfigurationNotApplied())
        {
            _configurationIsPending = false;
        }
    }
}
