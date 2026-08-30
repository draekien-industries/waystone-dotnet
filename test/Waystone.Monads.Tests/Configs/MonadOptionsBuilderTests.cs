namespace Waystone.Monads.Configs;

using System;
using FluentValidation.Configs;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadOptionsBuilder))]
public sealed class MonadOptionsBuilderTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenAnUnusableFallbackCode_WhenConfiguring_ThenRefuseIt(
        string? errorCode)
    {
        ArgumentException thrown = Should.Throw<ArgumentException>(
            () => MonadOptions.Create(o => o.UseFallbackErrorCode(errorCode!)));

        thrown.ParamName.ShouldBe("errorCode");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenAnUnusableFallbackMessage_WhenConfiguring_ThenRefuseIt(
        string? errorMessage)
    {
        ArgumentException thrown = Should.Throw<ArgumentException>(
            () => MonadOptions.Create(
                o => o.UseFallbackErrorMessage(errorMessage!)));

        thrown.ParamName.ShouldBe("errorMessage");
    }

    [Fact]
    public void GivenPaddedFallbacks_WhenConfiguring_ThenStoreThemTrimmed()
    {
        MonadOptions options = MonadOptions.Create(
            o => o.UseFallbackErrorCode("  padded.code  ")
               .UseFallbackErrorMessage("  padded message.  "));

        options.FallbackErrorCode.ShouldBe("padded.code");
        options.FallbackErrorMessage.ShouldBe("padded message.");
    }

    [Fact]
    public void GivenCancellationAsFailure_WhenConfiguring_ThenCatchIt()
    {
        MonadOptions options =
            MonadOptions.Create(o => o.UseCancellationAsFailure());

        options.Catches(new OperationCanceledException()).ShouldBeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenCancellationSetExplicitly_WhenConfiguring_ThenHonourTheValue(
        bool catchesCancellation)
    {
        MonadOptions options = MonadOptions.Create(
            o => o.UseCancellationAsFailure(catchesCancellation));

        options.Catches(new OperationCanceledException())
               .ShouldBe(catchesCancellation);
    }

    [Fact]
    public void GivenCancellationAlreadyCaught_WhenTurningItOff_ThenStopCatchingIt()
    {
        MonadOptions options = MonadOptions.Create(
            o => o.UseCancellationAsFailure()
                  .UseCancellationAsFailure(false));

        options.Catches(new OperationCanceledException()).ShouldBeFalse();
        options.Catches(new InvalidOperationException()).ShouldBeTrue();
    }

    [Fact]
    public void GivenTheDefaults_WhenBuilding_ThenLeaveCancellationAlone()
    {
        MonadOptions options = MonadOptions.Create(_ => { });

        options.Catches(new OperationCanceledException()).ShouldBeFalse();
        options.Catches(new InvalidOperationException()).ShouldBeTrue();
    }

    [Fact]
    public void
        GivenOneSatelliteConfiguredTwice_WhenBuilding_ThenBothCallsLandOnOneBuilder()
    {
        MonadValidationOptionsBuilder? first = null;
        MonadValidationOptionsBuilder? second = null;

        MonadOptions options = MonadOptions.Create(
            o =>
            {
                first = o.UseValidationErrorCode("first.code");
                second = o.UseValidationErrorCode("second.code");
            });

        second.ShouldBeSameAs(first);

        MonadValidationOptions.For(options)
                              .ValidationErrorCode.ShouldBe("second.code");
    }

    [Fact]
    public void
        GivenASatelliteConfigured_WhenReconfiguringOnlyACoreOption_ThenTheSatelliteSurvives()
    {
        MonadOptions configured =
            MonadOptions.Create(o => o.UseValidationErrorCode("carried.code"));

        MonadOptions rebuilt = configured.ToBuilder()
           .UseFallbackErrorCode("unrelated")
           .Build();

        MonadValidationOptions.For(rebuilt)
           .ValidationErrorCode.ShouldBe("carried.code");
    }

    [Fact]
    public void
        GivenASlotAllocatedAfterASnapshotWasBuilt_WhenReadingIt_ThenItIsNotConfigured()
    {
        MonadOptions snapshot = MonadOptions.Create(_ => { });

        int lateSlot = MonadOptionsSlot.Allocate();

        snapshot.Satellite<object>(lateSlot).ShouldBeNull();
    }
}
