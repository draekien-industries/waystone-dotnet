namespace Waystone.Monads.Fixtures;

using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Microsoft.Extensions.Logging;

/// <summary>
/// An <see cref="ILogger" /> that unpacks what the library logs back into the
/// <see cref="Exception" /> and <see cref="CallerInfo" /> pair the tests assert
/// on.
/// </summary>
/// <remarks>
/// The tests here observe handled exceptions to prove that <c>Try</c> swallowed
/// one — or did not — rather than to test logging. This keeps that observation
/// pointed at the supported configuration path now that
/// <c>UseExceptionLogger</c> is obsolete, without rewriting every assertion.
/// The diagnostic event would be the more direct probe and is the wrong one:
/// it is process-wide, so a scenario running in parallel would land in the
/// snapshot, whereas the logger follows the <c>MonadOptions</c> scope that set
/// it.
/// </remarks>
public sealed class HandledExceptionProbe(Action<Exception, CallerInfo> record)
    : ILogger
{
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull =>
        NoScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (exception is null
         || state is not IEnumerable<KeyValuePair<string, object?>> properties)
        {
            return;
        }

        Dictionary<string, object?> values =
            properties.ToDictionary(pair => pair.Key, pair => pair.Value);

        record(
            exception,
            new CallerInfo(
                values["MemberName"] as string ?? string.Empty,
                values["ArgumentExpression"] as string ?? string.Empty,
                values["LineNumber"] as int? ?? 0));
    }

    private sealed class NoScope : IDisposable
    {
        internal static readonly NoScope Instance = new();

        public void Dispose()
        { }
    }
}
