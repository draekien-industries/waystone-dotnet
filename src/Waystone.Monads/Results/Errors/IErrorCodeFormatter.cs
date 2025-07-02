namespace Waystone.Monads.Results.Errors;

using System;

/// <summary>
/// A formatter which converts a value of type <typeparamref name="T" />
/// to an error code string.
/// </summary>
/// <typeparam name="T">The type providing the error code</typeparam>
[Obsolete("Inherit from `ErrorCodeFactory` instead to configure global error code formatting behavior. This interface will be removed in a future version.")]
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