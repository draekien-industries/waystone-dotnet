namespace Waystone.Monads.Diagnostics;

using System;
using Configs;
using Fixtures;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(ConfigurationNotApplied))]
[Collection(GlobalMonadOptionsCollection.Name)]
public sealed class ConfigurationNotAppliedTests : IDisposable
{
    public ConfigurationNotAppliedTests()
    {
        MonadOptions.Reset();
    }

    /// <summary>
    /// Resets on the way out as well as in. Two of these tests publish a global
    /// snapshot, and the collection holds classes that assert on the default
    /// fallbacks without resetting first — <c>ErrorCodeTests</c> among them — so
    /// leaving the global dirty fails them rather than anything here.
    /// </summary>
    public void Dispose()
    {
        MonadOptions.Reset();
    }

    [Fact]
    public void GivenNoPendingConfiguration_WhenOptionsAreRead_ThenWriteNothing()
    {
        using var recorder = NewRecorder();

        Read();

        recorder.Recorded().ShouldBeEmpty();
    }

    [Fact]
    public void GivenPendingConfiguration_WhenOptionsAreRead_ThenWriteTheEvent()
    {
        using var recorder = NewRecorder();

        MonadOptions.MarkConfigurationPending();
        Read();

        recorder.Recorded().ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenPendingConfiguration_WhenOptionsAreReadTwice_ThenWriteOnce()
    {
        using var recorder = NewRecorder();

        MonadOptions.MarkConfigurationPending();
        Read();
        Read();
        Read();

        recorder.Recorded().ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenNoSubscriber_WhenOptionsAreRead_ThenHoldTheSignalForALaterOne()
    {
        MonadOptions.MarkConfigurationPending();
        Read();

        using var recorder = NewRecorder();
        Read();

        recorder.Recorded().ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenConfigurationIsInstalled_WhenOptionsAreRead_ThenWriteNothing()
    {
        MonadOptions.MarkConfigurationPending();
        MonadOptions.Install(
            MonadOptions.Create(builder => builder.UseFallbackErrorCode("Bound")));

        using var recorder = NewRecorder();
        Read();

        recorder.Recorded().ShouldBeEmpty();
    }

    [Fact]
    public void GivenConfigurationArrivesThroughConfigure_WhenOptionsAreRead_ThenWriteNothing()
    {
        MonadOptions.MarkConfigurationPending();
        MonadOptions.Configure(
            builder => builder.UseFallbackErrorCode("Configured"));

        using var recorder = NewRecorder();
        Read();

        recorder.Recorded().ShouldBeEmpty();
    }

    [Fact]
    public void GivenOptionsAreReset_WhenOptionsAreRead_ThenWriteNothing()
    {
        MonadOptions.MarkConfigurationPending();
        MonadOptions.Reset();

        using var recorder = NewRecorder();
        Read();

        recorder.Recorded().ShouldBeEmpty();
    }

    [Fact]
    public void GivenPendingConfiguration_WhenOptionsAreRead_ThenStillAnswerFromTheBootstrap()
    {
        using var recorder = NewRecorder();

        MonadOptions.MarkConfigurationPending();

        MonadOptions.Current.FallbackErrorCode.ShouldBe("Unspecified");
        recorder.Recorded().ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenTwoPayloads_WhenComparedForEquality_ThenTreatThemAsTheSame()
    {
        var payload = new ConfigurationNotApplied();
        var other = new ConfigurationNotApplied();

        payload.ShouldBe(other);
        payload.GetHashCode().ShouldBe(other.GetHashCode());
        (payload == other).ShouldBeTrue();
        (payload != other).ShouldBeFalse();
        payload.ToString().ShouldContain(nameof(ConfigurationNotApplied));
    }

    private static void Read()
    {
        _ = MonadOptions.Current;
    }

    private static EventRecorder<ConfigurationNotApplied> NewRecorder() =>
        new(MonadDiagnostics.ConfigurationNotAppliedEventName);
}
