namespace Waystone.Monads.Diagnostics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Options;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadDiagnosticEvent<>))]
public sealed class MonadDiagnosticEventTests
{
    private static MonadDiagnosticEvent<string> UniqueEvent() =>
        new($"Waystone.Monads.Tests.{Guid.NewGuid():N}");

    [Fact]
    public void GivenAnEvent_WhenReadingItsName_ThenGiveTheNameItWasBuiltWith()
    {
        var subject = new MonadDiagnosticEvent<string>("Waystone.Monads.Thing");

        subject.Name.ShouldBe("Waystone.Monads.Thing");
    }

    [Fact]
    public void GivenANullCallback_WhenSubscribing_ThenThrow()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();

        Should.Throw<ArgumentNullException>(() => subject.Subscribe(null!))
              .ParamName.ShouldBe("onEvent");
    }

    [Fact]
    public void GivenASubscription_WhenTheEventIsWritten_ThenPassThePayload()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();
        List<string> received = [];

        using IDisposable subscription = subject.Subscribe(received.Add);
        using var listener = new DiagnosticListener(
            MonadDiagnostics.ListenerName);

        listener.Write(subject.Name, "payload");

        received.ShouldBe(["payload"]);
    }

    [Fact]
    public void
        GivenASubscription_WhenTheListenerExistedFirst_ThenStillPassThePayload()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();
        List<string> received = [];

        using var listener = new DiagnosticListener(
            MonadDiagnostics.ListenerName);
        using IDisposable subscription = subject.Subscribe(received.Add);

        listener.Write(subject.Name, "payload");

        received.ShouldBe(["payload"]);
    }

    [Fact]
    public void GivenASubscription_WhenAnotherEventIsWritten_ThenIgnoreIt()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();
        MonadDiagnosticEvent<string> other = UniqueEvent();
        List<string> received = [];

        using IDisposable subscription = subject.Subscribe(received.Add);
        using var listener = new DiagnosticListener(
            MonadDiagnostics.ListenerName);

        listener.Write(other.Name, "payload");

        received.ShouldBeEmpty();
    }

    [Fact]
    public void GivenASubscription_WhenAnotherListenerWrites_ThenIgnoreIt()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();
        List<string> received = [];

        using IDisposable subscription = subject.Subscribe(received.Add);
        using var foreign = new DiagnosticListener("Some.Other.Listener");

        foreign.Write(subject.Name, "payload");

        received.ShouldBeEmpty();
    }

    [Fact]
    public void
        GivenASubscription_WhenThePayloadIsAnotherType_ThenSkipItRatherThanPassNull()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();
        List<string> received = [];

        using IDisposable subscription = subject.Subscribe(received.Add);
        using var listener = new DiagnosticListener(
            MonadDiagnostics.ListenerName);

        listener.Write(subject.Name, 42);
        listener.Write(subject.Name, "payload");

        received.ShouldBe(["payload"]);
    }

    [Fact]
    public void
        GivenTwoListenersOfTheSameName_WhenBothWrite_ThenPassBothPayloads()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();
        List<string> received = [];

        using IDisposable subscription = subject.Subscribe(received.Add);
        using var first = new DiagnosticListener(MonadDiagnostics.ListenerName);
        using var second =
            new DiagnosticListener(MonadDiagnostics.ListenerName);

        first.Write(subject.Name, "first");
        second.Write(subject.Name, "second");

        received.ShouldBe(["first", "second"]);
    }

    [Fact]
    public void GivenADisposedSubscription_WhenTheEventIsWritten_ThenIgnoreIt()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();
        List<string> received = [];

        IDisposable subscription = subject.Subscribe(received.Add);
        using var listener = new DiagnosticListener(
            MonadDiagnostics.ListenerName);

        listener.Write(subject.Name, "before");
        subscription.Dispose();
        listener.Write(subject.Name, "after");

        received.ShouldBe(["before"]);
    }

    [Fact]
    public void GivenADisposedSubscription_WhenDisposedAgain_ThenDoNothing()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();
        IDisposable subscription = subject.Subscribe(_ => { });

        subscription.Dispose();

        Should.NotThrow(subscription.Dispose);
    }

    [Fact]
    public void
        GivenADisposedSubscription_WhenALaterListenerAppears_ThenStayDetached()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();
        List<string> received = [];

        IDisposable subscription = subject.Subscribe(received.Add);
        subscription.Dispose();

        using var listener = new DiagnosticListener(
            MonadDiagnostics.ListenerName);

        listener.Write(subject.Name, "payload");

        received.ShouldBeEmpty();
    }

    [Fact]
    public void
        GivenADisposedSubscription_WhenAnAttachRacedTheDisposal_ThenStayDetached()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();
        List<string> received = [];

        IDisposable subscription = subject.Subscribe(received.Add);
        subscription.Dispose();

        using var listener = new DiagnosticListener(
            MonadDiagnostics.ListenerName);

        ((IObserver<DiagnosticListener>)subscription).OnNext(listener);
        listener.Write(subject.Name, "payload");

        received.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAThrowingCallback_WhenTheEventIsWritten_ThenLetItEscape()
    {
        MonadDiagnosticEvent<string> subject = UniqueEvent();

        using IDisposable subscription =
            subject.Subscribe(_ => throw new ProbeException());
        using var listener = new DiagnosticListener(
            MonadDiagnostics.ListenerName);

        Should.Throw<ProbeException>(
            () => listener.Write(subject.Name, "payload"));
    }

    [Fact]
    public void
        GivenTheExceptionHandledEvent_WhenTryCatchesAnException_ThenPassTheCallSite()
    {
        List<ExceptionHandled> received = [];

        using IDisposable subscription =
            MonadDiagnostics.ExceptionHandledEvent.Subscribe(
                handled =>
                {
                    if (handled.Exception is ProbeException)
                    {
                        received.Add(handled);
                    }
                });

        Option.Try<int>(() => throw new ProbeException());

        ExceptionHandled handled = received.ShouldHaveSingleItem();
        handled.Monad.ShouldBe(MonadKind.Option);
        handled.Caller.MemberName.ShouldBe(
            nameof(
                GivenTheExceptionHandledEvent_WhenTryCatchesAnException_ThenPassTheCallSite));
    }

    [Fact]
    public void GivenTheShippedEvents_WhenReadingTheirNames_ThenMatchTheConstants()
    {
        MonadDiagnostics.ExceptionHandledEvent.Name.ShouldBe(
            MonadDiagnostics.ExceptionHandledEventName);
        MonadDiagnostics.ScopeDisposedOutOfOrderEvent.Name.ShouldBe(
            MonadDiagnostics.ScopeDisposedOutOfOrderEventName);
        MonadDiagnostics.ConfigurationNotAppliedEvent.Name.ShouldBe(
            MonadDiagnostics.ConfigurationNotAppliedEventName);
    }

    private sealed class ProbeException() : Exception("Probe.");
}
