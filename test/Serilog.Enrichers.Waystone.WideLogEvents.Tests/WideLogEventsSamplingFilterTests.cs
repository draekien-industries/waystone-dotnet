namespace Serilog.Enrichers.Waystone.WideLogEvents.Tests;

using System;
using System.Linq;
using Events;
using NSubstitute;
using Parsing;
using Shouldly;
using Xunit;

public class WideLogEventsSamplingFilterTests
{
    [Fact]
    public void IsEnabled_WithException_AlwaysReturnsTrue()
    {
        // Arrange
        var options = new WideLogEventsSamplingOptions
        {
            VerboseSampleRate = 0.0,
            RandomDoubleProvider = Substitute.For<IRandomDoubleProvider>()
        };

        options.RandomDoubleProvider.NextDouble().Returns(1.0);

        var filter = new WideLogEventsSamplingFilter(options);
        var logEvent = CreateLogEvent(LogEventLevel.Verbose, new Exception());

        // Act
        bool enabled = filter.IsEnabled(logEvent);

        // Assert
        enabled.ShouldBeTrue();
    }

    [Theory]
    [InlineData(LogEventLevel.Verbose, 0.5, 0.4, true)]
    [InlineData(LogEventLevel.Verbose, 0.5, 0.5, true)]
    [InlineData(LogEventLevel.Verbose, 0.5, 0.6, false)]
    [InlineData(LogEventLevel.Debug, 0.5, 0.4, true)]
    [InlineData(LogEventLevel.Debug, 0.5, 0.6, false)]
    [InlineData(LogEventLevel.Information, 0.5, 0.4, true)]
    [InlineData(LogEventLevel.Information, 0.5, 0.6, false)]
    [InlineData(LogEventLevel.Warning, 0.5, 0.4, true)]
    [InlineData(LogEventLevel.Warning, 0.5, 0.6, false)]
    [InlineData(LogEventLevel.Error, 0.5, 0.4, true)]
    [InlineData(LogEventLevel.Error, 0.5, 0.6, false)]
    [InlineData(LogEventLevel.Fatal, 0.5, 0.4, true)]
    [InlineData(LogEventLevel.Fatal, 0.5, 0.6, false)]
    public void IsEnabled_BasedOnSampleRate(
        LogEventLevel level,
        double sampleRate,
        double randomValue,
        bool expected)
    {
        // Arrange
        var options = new WideLogEventsSamplingOptions();
        SetSampleRate(options, level, sampleRate);

        var mockProvider = Substitute.For<IRandomDoubleProvider>();
        mockProvider.NextDouble().Returns(randomValue);
        options.RandomDoubleProvider = mockProvider;

        var filter = new WideLogEventsSamplingFilter(options);
        var logEvent = CreateLogEvent(level);

        // Act
        bool enabled = filter.IsEnabled(logEvent);

        // Assert
        enabled.ShouldBe(expected);
    }

    private static void SetSampleRate(
        WideLogEventsSamplingOptions options,
        LogEventLevel level,
        double rate)
    {
        switch (level)
        {
            case LogEventLevel.Verbose:
                options.VerboseSampleRate = rate;

                break;
            case LogEventLevel.Debug:
                options.DebugSampleRate = rate;

                break;
            case LogEventLevel.Information:
                options.InformationSampleRate = rate;

                break;
            case LogEventLevel.Warning:
                options.WarningSampleRate = rate;

                break;
            case LogEventLevel.Error:
                options.ErrorSampleRate = rate;

                break;
            case LogEventLevel.Fatal:
                options.FatalSampleRate = rate;

                break;
        }
    }

    private static LogEvent CreateLogEvent(
        LogEventLevel level,
        Exception? exception = null)
    {
        return new LogEvent(
            DateTimeOffset.Now,
            level,
            exception,
            new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
            Enumerable.Empty<LogEventProperty>());
    }
}
