namespace Waystone.Monads.Configs;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Results.Errors;

/// <summary>Assembles the settings that become one <see cref="MonadOptions" /> snapshot.</summary>
/// <remarks>
/// You never construct one. <see cref="MonadOptions.Configure" /> and
/// <see cref="MonadOptions.BeginScope(Action{MonadOptionsBuilder})" /> hand you a builder seeded from the
/// options already in effect, and publish the result when your action returns —
/// so a setting you do not touch is inherited rather than reset.
/// <para>
/// A builder is not thread safe and is not meant to outlive the action it was
/// passed to. Keeping one and configuring it later changes nothing: the snapshot
/// was already built and published.
/// </para>
/// </remarks>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed class MonadOptionsBuilder
{
    private readonly object?[] _carried;

    private Dictionary<int, ISatelliteBuilder>? _overrides;

    internal MonadOptionsBuilder()
    {
        ErrorCodeFactory = new ErrorCodeFactory();
        FallbackErrorCode = "Unspecified";
        FallbackErrorMessage = "An unexpected error occurred.";
        CatchesCancellation = false;
        _carried = Array.Empty<object?>();
    }

    internal MonadOptionsBuilder(MonadOptions source)
    {
        ErrorCodeFactory = source.ErrorCodeFactory;
        FallbackErrorCode = source.FallbackErrorCode;
        FallbackErrorMessage = source.FallbackErrorMessage;
        CatchesCancellation = source.CatchesCancellation;
        _carried = source.Satellites;
    }

    internal ErrorCodeFactory ErrorCodeFactory { get; set; }
    internal string FallbackErrorCode { get; set; }
    internal string FallbackErrorMessage { get; set; }
    internal bool CatchesCancellation { get; set; }

    internal TBuilder Satellite<TBuilder>(
        int slot,
        Func<object?, TBuilder> create)
        where TBuilder : class, ISatelliteBuilder
    {
        Dictionary<int, ISatelliteBuilder> overrides =
            _overrides ??= new Dictionary<int, ISatelliteBuilder>();

        if (overrides.TryGetValue(slot, out ISatelliteBuilder? found))
        {
            return (TBuilder)found;
        }

        TBuilder builder = create(MonadOptionsSlot.At<object>(_carried, slot));
        overrides[slot] = builder;
        return builder;
    }

    internal MonadOptions Build() =>
        new(
            ErrorCodeFactory,
            FallbackErrorCode,
            FallbackErrorMessage,
            CatchesCancellation,
            BuildSatellites());

    /// <summary>
    /// Sets whether <c>Try</c> and <c>TryAsync</c> treat a cancellation as a
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
    /// <para>
    /// Pass <c>false</c> to put the setting back. A builder seeded from options
    /// configured elsewhere — by <see cref="MonadOptions.Configure" />, or by an
    /// earlier registration in a container — carries that decision forward, and
    /// passing <c>false</c> is the only way to reverse it.
    /// </para>
    /// </remarks>
    /// <param name="catchesCancellation">
    /// If true, a cancellation is caught and becomes a
    /// <see cref="Options.None{T}" /> or an <see cref="Results.Err{TOk,TErr}" />.
    /// If false, it propagates to the caller that requested it. Default: true,
    /// since turning the behaviour on is why you would call this. Absent any
    /// call at all the setting is false.
    /// </param>
    /// <returns>This builder, for chaining more configurations.</returns>
    public MonadOptionsBuilder UseCancellationAsFailure(
        bool catchesCancellation = true)
    {
        CatchesCancellation = catchesCancellation;
        return this;
    }

    /// <summary>
    /// Configures the factory that will be used to create
    /// <see cref="ErrorCode" /> instances from exceptions.
    /// </summary>
    /// <param name="factory">
    /// The implementation of <see cref="ErrorCodeFactory" /> you
    /// want the library to use.
    /// </param>
    /// <returns>This builder, for chaining more configurations.</returns>
    public MonadOptionsBuilder UseErrorCodeFactory(ErrorCodeFactory factory)
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
    /// <returns>This builder, for chaining more configurations.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="errorCode" /> is null, empty or whitespace. A fallback
    /// that is itself unusable would leave nothing to fall back to.
    /// </exception>
    public MonadOptionsBuilder UseFallbackErrorCode(string errorCode)
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
    /// <returns>This builder, for chaining more configurations.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="errorMessage" /> is null, empty or whitespace. A
    /// fallback that is itself unusable would leave nothing to fall back to.
    /// </exception>
    public MonadOptionsBuilder UseFallbackErrorMessage(string errorMessage)
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

    private object?[] BuildSatellites()
    {
        if (_overrides is null)
        {
            return _carried;
        }

        var satellites = new object?[MonadOptionsSlot.Count];

        Array.Copy(_carried, satellites, _carried.Length);

        foreach (KeyValuePair<int, ISatelliteBuilder> entry in _overrides)
        {
            satellites[entry.Key] = entry.Value.Build();
        }

        return satellites;
    }
}
