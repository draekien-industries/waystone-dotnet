namespace Serilog.Enrichers.Waystone.WideLogEvents;

using System.Diagnostics;
using Events;

/// <summary>
/// Represents configuration options for sampling wide log events.
/// This class allows you to control the rates at which log events
/// of various severity levels are included or excluded during
/// logging operations.
/// </summary>
public sealed class WideLogEventsSamplingOptions
{
    internal WideLogEventsSamplingOptions()
    {
        if (Debugger.IsAttached)
        {
            VerboseSampleRate = 1.0;
            DebugSampleRate = 1.0;
            InformationSampleRate = 1.0;
        }
        else
        {
            VerboseSampleRate = 0.01;
            DebugSampleRate = 0.01;
            InformationSampleRate = 0.05;
        }

        WarningSampleRate = 0.1;
        ErrorSampleRate = 1.0;
        FatalSampleRate = 1.0;
        RandomDoubleProvider = new InternalRandomDoubleProvider();
    }

    /// <summary>
    /// Determines the sampling rate for log events with a <see cref="LogEventLevel" />
    /// of
    /// <see cref="LogEventLevel.Verbose" /> during logging.
    /// </summary>
    /// <remarks>
    /// The value should be a double between <c>0.0</c> (representing no events
    /// sampled)
    /// and <c>1.0</c> (representing all events sampled). Values outside this range are
    /// clamped
    /// to the nearest valid boundary.
    /// </remarks>
    /// <value>
    /// The sampling rate as a <see cref="double" />. Default values are set depending
    /// on whether
    /// a debugger is attached: <c>1.0</c> if attached, otherwise <c>0.01</c>.
    /// </value>
    public double VerboseSampleRate
    {
        get;
        set
        {
            field = value switch
            {
                > 1.0 => 1.0,
                < 0.0 => 0.0,
                var _ => value,
            };
        }
    }

    /// <summary>
    /// Determines the sampling rate for log events with a <see cref="LogEventLevel" />
    /// of <see cref="LogEventLevel.Debug" /> during logging.
    /// </summary>
    /// <remarks>
    /// The value should be a double between <c>0.0</c> (representing no events
    /// sampled)
    /// and <c>1.0</c> (representing all events sampled). Values outside this range are
    /// clamped to the nearest valid boundary.
    /// </remarks>
    /// <value>
    /// The sampling rate as a <see cref="double" />. Default values are set depending
    /// on whether a debugger is attached: <c>1.0</c> if attached, otherwise
    /// <c>0.01</c>.
    /// </value>
    public double DebugSampleRate
    {
        get;
        set
        {
            field = value switch
            {
                > 1.0 => 1.0,
                < 0.0 => 0.0,
                var _ => value,
            };
        }
    }

    /// <summary>
    /// Determines the sampling rate for log events with a <see cref="LogEventLevel" />
    /// of <see cref="LogEventLevel.Information" /> during logging.
    /// </summary>
    /// <remarks>
    /// The value should be a double between <c>0.0</c> (representing no events
    /// sampled) and <c>1.0</c> (representing all events sampled). Values outside this
    /// range are clamped to the nearest valid boundary.
    /// </remarks>
    /// <value>
    /// The sampling rate as a <see cref="double" />. Default values are set depending
    /// on whether a debugger is attached: <c>1.0</c> if attached, otherwise
    /// <c>0.05</c>.
    /// </value>
    public double InformationSampleRate
    {
        get;
        set
        {
            field = value switch
            {
                > 1.0 => 1.0,
                < 0.0 => 0.0,
                var _ => value,
            };
        }
    }

    /// <summary>
    /// Determines the sampling rate for log events with a <see cref="LogEventLevel" />
    /// of <see cref="LogEventLevel.Warning" /> during logging.
    /// </summary>
    /// <remarks>
    /// The value should be a double between <c>0.0</c> (representing no events
    /// sampled)
    /// and <c>1.0</c> (representing all events sampled). Values outside this range are
    /// clamped
    /// to the nearest valid boundary.
    /// </remarks>
    /// <value>
    /// The sampling rate as a <see cref="double" />. Default value is set to
    /// <c>0.1</c>.
    /// </value>
    public double WarningSampleRate
    {
        get;
        set
        {
            field = value switch
            {
                > 1.0 => 1.0,
                < 0.0 => 0.0,
                var _ => value,
            };
        }
    }

    /// <summary>
    /// Determines the sampling rate for log events with a <see cref="LogEventLevel" />
    /// of <see cref="LogEventLevel.Error" /> during logging.
    /// </summary>
    /// <remarks>
    /// The value should be a double between <c>0.0</c> (representing no events
    /// sampled) and <c>1.0</c> (representing all events sampled). Values outside this
    /// range
    /// are clamped to the nearest valid boundary.
    /// </remarks>
    /// <value>
    /// The sampling rate as a <see cref="double" />. The default value is <c>1.0</c>.
    /// </value>
    public double ErrorSampleRate
    {
        get;
        set
        {
            field = value switch
            {
                > 1.0 => 1.0,
                < 0.0 => 0.0,
                var _ => value,
            };
        }
    }

    /// <summary>
    /// Determines the sampling rate for log events with a <see cref="LogEventLevel" />
    /// of <see cref="LogEventLevel.Fatal" /> during logging.
    /// </summary>
    /// <remarks>
    /// The value should be a double between <c>0.0</c> (representing no events
    /// sampled) and <c>1.0</c> (representing all events sampled). Values outside this
    /// range are
    /// clamped to the nearest valid boundary.
    /// </remarks>
    /// <value>
    /// The sampling rate as a <see cref="double" />. Default values are set depending
    /// on whether a debugger is attached: <c>1.0</c> if attached, otherwise <c>1.0</c>
    /// .
    /// </value>
    public double FatalSampleRate
    {
        get;
        set
        {
            field = value switch
            {
                > 1.0 => 1.0,
                < 0.0 => 0.0,
                var _ => value,
            };
        }
    }

    /// <summary>
    /// Provides an implementation of <see cref="IRandomDoubleProvider" /> to generate
    /// random double values.
    /// </summary>
    /// <remarks>
    /// This property allows customization of the random double generation mechanism
    /// used internally
    /// by the sampling logic. By default, it is assigned an instance of
    /// <see cref="InternalRandomDoubleProvider" />,
    /// but you can substitute it with a custom implementation.
    /// </remarks>
    /// <value>
    /// The current implementation of <see cref="IRandomDoubleProvider" /> used to
    /// create random double values
    /// for sampling purposes.
    /// </value>
    public IRandomDoubleProvider RandomDoubleProvider { get; set; }
}
