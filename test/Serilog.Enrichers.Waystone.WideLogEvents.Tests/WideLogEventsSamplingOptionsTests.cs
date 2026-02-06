namespace Serilog.Enrichers.Waystone.WideLogEvents.Tests;

using Shouldly;
using Xunit;

public class WideLogEventsSamplingOptionsTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var options = new WideLogEventsSamplingOptions();

        // Assert
        options.VerboseSampleRate.ShouldBe(0.01);
        options.DebugSampleRate.ShouldBe(0.01);
        options.InformationSampleRate.ShouldBe(0.05);
        options.WarningSampleRate.ShouldBe(0.1);
        options.ErrorSampleRate.ShouldBe(1.0);
        options.FatalSampleRate.ShouldBe(1.0);
        options.RandomDoubleProvider.ShouldNotBeNull();

        options.RandomDoubleProvider
           .ShouldBeOfType<InternalRandomDoubleProvider>();
    }

    [Theory]
    [InlineData(1.5, 1.0)]
    [InlineData(1.0, 1.0)]
    [InlineData(0.5, 0.5)]
    [InlineData(0.0, 0.0)]
    [InlineData(-0.1, 0.0)]
    public void SampleRates_ClampValues(double input, double expected)
    {
        // Arrange
        var options = new WideLogEventsSamplingOptions();

        // Act & Assert
        options.VerboseSampleRate = input;
        options.VerboseSampleRate.ShouldBe(expected);

        options.DebugSampleRate = input;
        options.DebugSampleRate.ShouldBe(expected);

        options.InformationSampleRate = input;
        options.InformationSampleRate.ShouldBe(expected);

        options.WarningSampleRate = input;
        options.WarningSampleRate.ShouldBe(expected);

        options.ErrorSampleRate = input;
        options.ErrorSampleRate.ShouldBe(expected);

        options.FatalSampleRate = input;
        options.FatalSampleRate.ShouldBe(expected);
    }
}
