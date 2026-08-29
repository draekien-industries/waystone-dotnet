namespace Waystone.Monads.Configs;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Diagnostics;
using FluentValidation.Configs;
using Fixtures;
using JetBrains.Annotations;
using Results.Errors;
using Shouldly;
using Xunit;

[Collection(GlobalMonadOptionsCollection.Name)]
[TestSubject(typeof(MonadOptionsScope))]
public sealed class MonadOptionsScopeTests
{
    private static string ResolveFallbackCode() => new ErrorCode(" ").Value;

    private static string ResolveFallbackMessage() =>
        new Error("code", " ").Message;

    [Fact]
    public void GivenScope_WhenResolvingOptions_ThenUseScopedValue()
    {
        using (MonadOptions.BeginScope(o => o.UseFallbackErrorCode("scoped")))
        {
            ResolveFallbackCode().ShouldBe("scoped");
        }
    }

    [Fact]
    public void GivenScopeHasEnded_WhenResolvingOptions_ThenUseGlobalValue()
    {
        string global = ResolveFallbackCode();

        using (MonadOptions.BeginScope(o => o.UseFallbackErrorCode("scoped")))
        {
            ResolveFallbackCode().ShouldBe("scoped");
        }

        ResolveFallbackCode().ShouldBe(global);
    }

    [Fact]
    public void GivenScope_WhenOptionIsNotOverridden_ThenInheritGlobalValue()
    {
        string globalMessage = ResolveFallbackMessage();

        using (MonadOptions.BeginScope(o => o.UseFallbackErrorCode("scoped")))
        {
            ResolveFallbackCode().ShouldBe("scoped");
            ResolveFallbackMessage().ShouldBe(globalMessage);
        }
    }

    [Fact]
    public void GivenNestedScopes_WhenInnerEnds_ThenRestoreOuterScope()
    {
        using (MonadOptions.BeginScope(o => o.UseFallbackErrorCode("outer")))
        {
            ResolveFallbackCode().ShouldBe("outer");

            using (MonadOptions.BeginScope(
                o => o.UseFallbackErrorCode("inner")))
            {
                ResolveFallbackCode().ShouldBe("inner");
            }

            ResolveFallbackCode().ShouldBe("outer");
        }
    }

    [Fact]
    public async Task GivenScope_WhenAwaiting_ThenScopeFlowsAcrossAwait()
    {
        using (MonadOptions.BeginScope(o => o.UseFallbackErrorCode("scoped")))
        {
            await Task.Yield();
            ResolveFallbackCode().ShouldBe("scoped");

            await Task.Run(
                () => ResolveFallbackCode().ShouldBe("scoped"),
                TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task
        GivenConcurrentScopes_WhenResolvingOptions_ThenEachFlowIsIsolated()
    {
        async Task<string> Resolve(string code)
        {
            using (MonadOptions.BeginScope(o => o.UseFallbackErrorCode(code)))
            {
                await Task.Yield();
                await Task.Delay(5, TestContext.Current.CancellationToken);

                return ResolveFallbackCode();
            }
        }

        string[] resolved = await Task.WhenAll(
            Resolve("first"),
            Resolve("second"),
            Resolve("third"));

        resolved.ShouldBe(["first", "second", "third"]);
    }

    [Fact]
    public void GivenScope_WhenScopeEnds_ThenGlobalIsNotMutated()
    {
        string global = ResolveFallbackCode();

        using (MonadOptions.BeginScope(o => o.UseFallbackErrorCode("scoped")))
        { }

        MonadOptions.Global.FallbackErrorCode.ShouldBe(global);
    }

    [Fact]
    public void GivenScope_WhenOverridingErrorCodeFactory_ThenUseScopedFactory()
    {
        InvalidOperationException exception = new();

        string global = ErrorCode.FromException(exception).Value;

        using (MonadOptions.BeginScope(
            o => o.UseErrorCodeFactory(new PrefixingErrorCodeFactory())))
        {
            ErrorCode.FromException(exception)
               .Value.ShouldBe("scoped.InvalidOperation");
        }

        ErrorCode.FromException(exception).Value.ShouldBe(global);
    }

    [Fact]
    public void
        GivenScope_WhenOverridingValidationOptions_ThenUseScopedValidationOptions()
    {
        using (MonadOptions.BeginScope(
            o => o.UseValidationErrorCode("scoped.validation")))
        {
            MonadValidationOptions.Current.ValidationErrorCode.ShouldBe(
                "scoped.validation");
        }

        MonadValidationOptions.Current.ValidationErrorCode.ShouldNotBe(
            "scoped.validation");
    }

    [Fact]
    public void GivenPrebuiltOptions_WhenBeginningScope_ThenUseThoseOptions()
    {
        MonadOptions options =
            MonadOptions.Create(o => o.UseFallbackErrorCode("prebuilt"));

        using (MonadOptions.BeginScope(options))
        {
            ResolveFallbackCode().ShouldBe("prebuilt");
        }

        using (MonadOptions.BeginScope(options))
        {
            ResolveFallbackCode().ShouldBe("prebuilt");
        }
    }

    [Fact]
    public void
        GivenScopeIsOpen_WhenConfiguringGlobal_ThenScopeKeepsItsSnapshot()
    {
        string original = MonadOptions.Global.FallbackErrorCode;

        try
        {
            using (MonadOptions.BeginScope(
                o => o.UseFallbackErrorCode("scoped")))
            {
                MonadOptions.Configure(
                    o => o.UseFallbackErrorCode("reconfigured"));

                ResolveFallbackCode().ShouldBe("scoped");
            }

            ResolveFallbackCode().ShouldBe("reconfigured");
        }
        finally
        {
            MonadOptions.Configure(o => o.UseFallbackErrorCode(original));
        }
    }

    [Fact]
    public void GivenNestedScopes_WhenDisposedInReverse_ThenWriteNothing()
    {
        using var recorder = new ScopeEventRecorder("reverse");

        MonadOptionsScope outer = BeginScope("reverse.outer");
        MonadOptionsScope inner = BeginScope("reverse.inner");

        inner.Dispose();
        outer.Dispose();

        recorder.Recorded().ShouldBeEmpty();
        ResolveFallbackCode().ShouldNotStartWith("reverse.");
    }

    [Fact]
    public void
        GivenNestedScopes_WhenTheOuterIsDisposedFirst_ThenLeaveTheInnerInEffect()
    {
        MonadOptionsScope outer = BeginScope("early.outer");
        BeginScope("early.inner");

        outer.Dispose();

        ResolveFallbackCode().ShouldBe("early.inner");

        ClearScopedOptions();
    }

    [Fact]
    public void
        GivenNestedScopes_WhenTheOuterIsDisposedFirst_ThenWriteTheOutOfOrderEvent()
    {
        using var recorder = new ScopeEventRecorder("event");

        MonadOptions outerOptions = Options("event.outer");
        MonadOptions innerOptions = Options("event.inner");

        MonadOptionsScope outer = MonadOptions.BeginScope(outerOptions);
        MonadOptions.BeginScope(innerOptions);

        outer.Dispose();

        ScopeDisposedOutOfOrder written =
            recorder.Recorded().ShouldHaveSingleItem();
        written.Scope.ShouldBeSameAs(outerOptions);
        written.Live.ShouldBeSameAs(innerOptions);

        ClearScopedOptions();
    }

    [Fact]
    public void
        GivenTheOuterWasDisposedFirst_WhenTheInnerIsDisposed_ThenTheOuterOptionsOutliveTheirScope()
    {
        MonadOptionsScope outer = BeginScope("outlive.outer");
        MonadOptionsScope inner = BeginScope("outlive.inner");

        outer.Dispose();
        inner.Dispose();

        ResolveFallbackCode().ShouldBe("outlive.outer");

        ClearScopedOptions();
    }

    [Fact]
    public void GivenAScope_WhenDisposedTwice_ThenWriteNothing()
    {
        using var recorder = new ScopeEventRecorder("twice");

        string global = ResolveFallbackCode();

        MonadOptionsScope scope = BeginScope("twice.scoped");

        scope.Dispose();
        scope.Dispose();

        recorder.Recorded().ShouldBeEmpty();
        ResolveFallbackCode().ShouldBe(global);
    }

    [Fact]
    public void
        GivenANestedScope_WhenDisposedTwice_ThenLeaveTheOuterScopeInEffect()
    {
        using var recorder = new ScopeEventRecorder("nestedtwice");

        BeginScope("nestedtwice.outer");
        MonadOptionsScope inner = BeginScope("nestedtwice.inner");

        inner.Dispose();
        inner.Dispose();

        recorder.Recorded().ShouldBeEmpty();
        ResolveFallbackCode().ShouldBe("nestedtwice.outer");

        ClearScopedOptions();
    }

    [Fact]
    public void
        GivenALiveScope_WhenADefaultScopeIsDisposed_ThenLeaveTheLiveScopeInEffect()
    {
        using var recorder = new ScopeEventRecorder("default");

        MonadOptions live = Options("default.live");
        MonadOptions.BeginScope(live);

        default(MonadOptionsScope).Dispose();

        ResolveFallbackCode().ShouldBe("default.live");

        ScopeDisposedOutOfOrder written =
            recorder.Recorded().ShouldHaveSingleItem();
        written.Scope.ShouldBeNull();
        written.Live.ShouldBeSameAs(live);

        ClearScopedOptions();
    }

    [Fact]
    public void GivenNoScope_WhenADefaultScopeIsDisposed_ThenWriteNothing()
    {
        using var recorder = new ScopeEventRecorder("nolive");

        string global = ResolveFallbackCode();

        default(MonadOptionsScope).Dispose();

        recorder.Recorded().ShouldBeEmpty();
        ResolveFallbackCode().ShouldBe(global);
    }

    [Fact]
    public void
        GivenTheOuterAlreadyDeclined_WhenDisposedAgain_ThenWriteTheEventAgain()
    {
        using var recorder = new ScopeEventRecorder("again");

        MonadOptionsScope outer = BeginScope("again.outer");
        BeginScope("again.inner");

        outer.Dispose();
        outer.Dispose();

        recorder.Recorded().Count.ShouldBe(2);

        ClearScopedOptions();
    }

    private static MonadOptions Options(string code) =>
        MonadOptions.Create(o => o.UseFallbackErrorCode(code));

    private static MonadOptionsScope BeginScope(string code) =>
        MonadOptions.BeginScope(Options(code));

    private static void ClearScopedOptions()
    {
        MonadOptions.ScopedOptions.Value = null;
    }

    private sealed class PrefixingErrorCodeFactory : ErrorCodeFactory
    {
        public override ErrorCode FromException(Exception exception) =>
            new($"scoped.{base.FromException(exception).Value}");
    }

    /// <summary>
    /// Collects the out-of-order disposal events whose options carry this test's
    /// own fallback error code prefix.
    /// </summary>
    /// <remarks>
    /// Subscribes the way a consumer does, through
    /// <see cref="DiagnosticListener.AllListeners" />. The listener is
    /// process-wide, so the prefix filter is what keeps a test in another
    /// collection from landing an event in this one's snapshot — and unlike the
    /// handled-exception events, these carry no exception type to filter on.
    /// </remarks>
    private sealed class ScopeEventRecorder : IDisposable
    {
        private readonly IDisposable _allListeners;

        private readonly ConcurrentQueue<ScopeDisposedOutOfOrder> _events =
            new();

        private readonly object _gate = new();

        private readonly string _prefix;

        private readonly List<IDisposable> _subscriptions = new();

        public ScopeEventRecorder(string prefix)
        {
            _prefix = $"{prefix}.";

            _allListeners = DiagnosticListener.AllListeners.Subscribe(
                new Observer<DiagnosticListener>(Attach));
        }

        public void Dispose()
        {
            lock (_gate)
            {
                foreach (IDisposable subscription in _subscriptions)
                {
                    subscription.Dispose();
                }

                _subscriptions.Clear();
            }

            _allListeners.Dispose();
        }

        public IReadOnlyList<ScopeDisposedOutOfOrder> Recorded() =>
            _events.Where(
                        written => Mine(written.Scope) || Mine(written.Live))
                   .ToList();

        private bool Mine(MonadOptions? options) =>
            options?.FallbackErrorCode.StartsWith(
                _prefix,
                StringComparison.Ordinal)
         == true;

        private void Attach(DiagnosticListener listener)
        {
            if (listener.Name != MonadDiagnostics.ListenerName)
            {
                return;
            }

            lock (_gate)
            {
                _subscriptions.Add(
                    listener.Subscribe(
                        new Observer<KeyValuePair<string, object?>>(Record),
                        name => name
                             == MonadDiagnostics
                                   .ScopeDisposedOutOfOrderEventName));
            }
        }

        private void Record(KeyValuePair<string, object?> written)
        {
            if (written.Key == MonadDiagnostics.ScopeDisposedOutOfOrderEventName
             && written.Value is ScopeDisposedOutOfOrder disposed)
            {
                _events.Enqueue(disposed);
            }
        }
    }
}
