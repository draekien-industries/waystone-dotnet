namespace Serilog.Enrichers.Waystone.WideLogEvents;

using System;

/// <summary>
/// Provides an internal implementation of <see cref="IRandomDoubleProvider" />
/// for generating random double-precision floating-point numbers.
/// </summary>
/// <remarks>
/// This class utilizes the <see cref="Random" /> class to generate random double
/// values,
/// typically in the range [0.0, 1.0). It is used internally for scenarios where
/// sampling logic requires randomness.
/// </remarks>
/// <seealso cref="IRandomDoubleProvider" />
internal sealed class InternalRandomDoubleProvider : IRandomDoubleProvider
{
    /// <inheritdoc />
    public double NextDouble() => new Random().NextDouble();
}
