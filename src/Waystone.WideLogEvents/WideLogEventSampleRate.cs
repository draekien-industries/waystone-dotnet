namespace Waystone.WideLogEvents;

public sealed class WideLogEventSampleRate
{
    public double Success { get; set; } = 0.05;

    public double Failure { get; set; } = 1.0;

    public double Indeterminate { get; set; } = 1.0;
}
