namespace Waystone.Monads.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Configs;
using Diagnostics;
using Extensions.Logging.Configs;
using Fixtures;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Options;
using Results;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadLoggingOptions))]
public sealed class MonadLoggingOptionsTests
{
    [Fact]
    public void GivenAConfiguredLogger_WhenOptionTryThrows_ThenLogTheException()
    {
        var logger = new RecordingLogger();

        using (MonadOptions.BeginScope(options => options.UseLogger(logger)))
        {
            Option.Try<int>(() => throw new ProbeException());
        }

        Entry entry = logger.Entries.ShouldHaveSingleItem();
        entry.Exception.ShouldBeOfType<ProbeException>();
        entry.Level.ShouldBe(LogLevel.Debug);
    }

    [Fact]
    public void GivenAConfiguredLogger_WhenResultTryThrows_ThenLogTheException()
    {
        var logger = new RecordingLogger();

        using (MonadOptions.BeginScope(options => options.UseLogger(logger)))
        {
            Result.Try<int>(() => throw new ProbeException());
        }

        logger.Entries.ShouldHaveSingleItem()
              .Exception.ShouldBeOfType<ProbeException>();
    }

    [Fact]
    public void GivenAConfiguredLogger_WhenOptionTryThrows_ThenCarryTheCallSite()
    {
        var logger = new RecordingLogger();

        using (MonadOptions.BeginScope(options => options.UseLogger(logger)))
        {
            Option.Try<int>(() => throw new ProbeException());
        }

        IReadOnlyDictionary<string, object?> state =
            logger.Entries.ShouldHaveSingleItem().State;

        state["MemberName"]
           .ShouldBe(
                nameof(
                    GivenAConfiguredLogger_WhenOptionTryThrows_ThenCarryTheCallSite));
        state["ArgumentExpression"].ShouldNotBeNull();
        state["LineNumber"].ShouldBeOfType<int>().ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GivenALevel_WhenOptionTryThrows_ThenLogAtThatLevel()
    {
        var logger = new RecordingLogger();

        using (MonadOptions.BeginScope(
                   options => options.UseLogger(logger, LogLevel.Warning)))
        {
            Option.Try<int>(() => throw new ProbeException());
        }

        logger.Entries.ShouldHaveSingleItem().Level.ShouldBe(LogLevel.Warning);
    }

    [Fact]
    public void GivenALoggerThatRejectsTheLevel_WhenOptionTryThrows_ThenWriteNothing()
    {
        var logger = new RecordingLogger(false);

        using (MonadOptions.BeginScope(options => options.UseLogger(logger)))
        {
            Option.Try<int>(() => throw new ProbeException());
        }

        logger.Entries.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAScope_WhenItEnds_ThenStopLoggingToItsLogger()
    {
        var logger = new RecordingLogger();

        using (MonadOptions.BeginScope(options => options.UseLogger(logger)))
        { }

        Option.Try<int>(() => throw new ProbeException());

        logger.Entries.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAFactory_WhenConfigured_ThenUseTheLibrarysOwnCategory()
    {
        var factory = new RecordingLoggerFactory();

        using (MonadOptions.BeginScope(
                   options => options.UseLoggerFactory(factory)))
        {
            Option.Try<int>(() => throw new ProbeException());
        }

        factory.Categories.ShouldBe([MonadLoggingOptions.LoggerCategory]);
    }

    [Fact]
    public void GivenAProviderWithAFactory_WhenConfigured_ThenResolveIt()
    {
        var factory = new RecordingLoggerFactory();
        var provider = new StubProvider(factory);

        using (MonadOptions.BeginScope(
                   options => options.UseLoggerFactoryFrom(provider)))
        {
            Option.Try<int>(() => throw new ProbeException());
        }

        factory.Loggers.ShouldHaveSingleItem()
               .Entries.ShouldHaveSingleItem()
               .Exception.ShouldBeOfType<ProbeException>();
    }

    [Fact]
    public void GivenAProviderWithoutAFactory_WhenConfigured_ThenSayWhatToCallInstead()
    {
        var provider = new StubProvider(null);

        InvalidOperationException thrown =
            Should.Throw<InvalidOperationException>(
                () => MonadOptions.BeginScope(
                    options => options.UseLoggerFactoryFrom(provider)));

        thrown.Message.ShouldContain(
            nameof(MonadOptionsBuilderExtensions.UseLoggerFactory));
    }

    [Fact]
    public void GivenANullLogger_WhenConfigured_ThenRefuseIt()
    {
        Should.Throw<ArgumentNullException>(
            () => MonadOptions.BeginScope(
                options => options.UseLogger(null!)));
    }

    [Fact]
    public void GivenANullFactory_WhenConfigured_ThenRefuseIt()
    {
        Should.Throw<ArgumentNullException>(
            () => MonadOptions.BeginScope(
                options => options.UseLoggerFactory(null!)));
    }

    [Fact]
    public void GivenSomeoneElsesPayload_WhenWrittenToTheEvent_ThenIgnoreIt()
    {
        var logger = new RecordingLogger();

        using (MonadOptions.BeginScope(options => options.UseLogger(logger)))
        {
            MonadDiagnostics.Listener.Write(
                MonadDiagnostics.ExceptionHandledEventName,
                "not an ExceptionHandled");
        }

        logger.Everything.ShouldBeEmpty();
    }

    /// <summary>
    /// The subscription is to <c>AllListeners</c>, so every listener anywhere in
    /// the process reaches the logger's <c>Attach</c> and only the name keeps
    /// someone else's events out. Nothing else covers that filter: a foreign
    /// listener has to exist for it to be reached at all.
    /// </summary>
    [Fact]
    public void GivenSomeoneElsesListener_WhenItWritesTheEvent_ThenIgnoreIt()
    {
        var logger = new RecordingLogger();

        using (MonadOptions.BeginScope(options => options.UseLogger(logger)))
        using (var foreign = new DiagnosticListener("Someone.Elses.Listener"))
        {
            foreign.Write(
                MonadDiagnostics.ExceptionHandledEventName,
                new ExceptionHandled(
                    new ProbeException(),
                    new CallerInfo("Member", "expression", 1),
                    MonadKind.Option));
        }

        logger.Everything.ShouldBeEmpty();
    }

    private sealed class ProbeException() : Exception("Probe.");

    private sealed class Entry(
        LogLevel level,
        Exception? exception,
        IReadOnlyDictionary<string, object?> state)
    {
        public LogLevel Level { get; } = level;

        public Exception? Exception { get; } = exception;

        public IReadOnlyDictionary<string, object?> State { get; } = state;
    }

    private sealed class RecordingLogger(bool enabled = true) : ILogger
    {
        private readonly ConcurrentQueue<Entry> _entries = new();

        public IReadOnlyList<Entry> Entries =>
            _entries.Where(entry => entry.Exception is ProbeException).ToList();

        public IReadOnlyList<Entry> Everything => _entries.ToList();

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull =>
            NoScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => enabled;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Dictionary<string, object?> properties =
                state is IEnumerable<KeyValuePair<string, object?>> pairs
                    ? pairs.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value)
                    : new Dictionary<string, object?>();

            _entries.Enqueue(new Entry(logLevel, exception, properties));
        }
    }

    private sealed class RecordingLoggerFactory : ILoggerFactory
    {
        private readonly List<string> _categories = [];

        private readonly List<RecordingLogger> _loggers = [];

        public IReadOnlyList<string> Categories => _categories;

        public IReadOnlyList<RecordingLogger> Loggers => _loggers;

        public void AddProvider(ILoggerProvider provider)
        { }

        public ILogger CreateLogger(string categoryName)
        {
            _categories.Add(categoryName);
            var logger = new RecordingLogger();
            _loggers.Add(logger);
            return logger;
        }

        public void Dispose()
        { }
    }

    private sealed class StubProvider(ILoggerFactory? factory) : IServiceProvider
    {
        public object? GetService(Type serviceType) =>
            serviceType == typeof(ILoggerFactory) ? factory : null;
    }
}
