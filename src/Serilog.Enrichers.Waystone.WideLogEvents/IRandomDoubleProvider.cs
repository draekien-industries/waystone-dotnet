namespace Serilog.Enrichers.Waystone.WideLogEvents;

/// <summary>
/// Represents a provider for generating random double-precision floating-point
/// numbers.
/// </summary>
/// <remarks>
/// This interface is designed to provide a mechanism for obtaining random double
/// values,
/// typically in the range [0.0, 1.0). It allows decoupling the random number
/// generation logic
/// from the consuming code, facilitating testing and custom implementations.
/// </remarks>
public interface IRandomDoubleProvider
{
    /// <summary>
    /// Generates a random double-precision floating-point number.
    /// </summary>
    /// <remarks>
    /// The generated value is typically in the range [0.0, 1.0).
    /// This method provides a mechanism for obtaining random double values,
    /// which can be used for sampling, algorithms, or other use cases requiring
    /// randomness.
    /// </remarks>
    /// <returns>
    /// A <see cref="double" /> representing a randomly generated value.
    /// </returns>
    double NextDouble();
}
