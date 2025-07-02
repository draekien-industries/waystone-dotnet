namespace Waystone.Monads.Results.Errors;

/// <summary>
/// A formatter which converts a value of type <typeparamref name="T" />
/// to an error code string.
/// </summary>
/// <typeparam name="T">The type providing the error code</typeparam>
public interface IErrorCodeFormatter<in T>
{
    /// <summary>
    /// Formats a value of type <typeparamref name="T" /> to an error code
    /// string.
    /// </summary>
    /// <param name="value">The value to format</param>
    /// <returns>The error code string</returns>
    string Format(T value);
}