namespace Waystone.Monads.Results.Errors;

using System;
using Configs;

/// <summary>A short code representing an error type in the application.</summary>
/// <remarks>
/// Two instances are equal when their <see cref="Value" /> strings match
/// exactly; the comparison is ordinal and case-sensitive, so <c>order.NotFound</c>
/// and <c>ORDER.NOTFOUND</c> are different codes. Keep a code stable across
/// occurrences of the same error type, since consumers branch on it.
/// </remarks>
public sealed record ErrorCode
{
    /// <summary>
    /// Creates a new instance of <see cref="ErrorCode" /> from a string
    /// value.
    /// </summary>
    /// <remarks>
    /// Surrounding whitespace is trimmed off. A value that is null, empty or
    /// whitespace is replaced by the fallback configured through
    /// <see cref="MonadOptionsBuilder.UseFallbackErrorCode" />, so this never throws and
    /// <see cref="Value" /> is never null or blank. Default fallback:
    /// <c>Unspecified</c>.
    /// </remarks>
    /// <param name="value">The error code string value</param>
    public ErrorCode(string value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? MonadOptions.Current.FallbackErrorCode
            : value.Trim();
    }

    /// <summary>The error code string value, trimmed and never null or blank.</summary>
    public string Value { get; }

    /// <summary>Creates an instance of an <see cref="ErrorCode" /> from an exception.</summary>
    /// <remarks>
    /// Prefer an <c>[ErrorCodeCatalog]</c> enum. The code here is the exception's
    /// type name with a trailing <c>Exception</c> removed —
    /// <see cref="InvalidOperationException" /> gives <c>InvalidOperation</c> — so
    /// renaming or swapping the exception type silently changes the code a consumer
    /// observes. <see cref="Exception" /> itself is left as <c>Exception</c>. Uses
    /// the <see cref="ErrorCodeFactory" /> configured in
    /// <see cref="MonadOptions" />, so a custom factory changes all of this.
    /// </remarks>
    /// <param name="exception">The exception to take the code from.</param>
    /// <returns>The created instance of <see cref="ErrorCode" />.</returns>
    public static ErrorCode FromException(Exception exception) =>
        MonadOptions.Current.ErrorCodeFactory.FromException(exception);

    /// <summary>
    /// Implicitly converts an <see cref="ErrorCode" /> instance to its string
    /// representation.
    /// </summary>
    /// <param name="value">The <see cref="ErrorCode" /> instance to be converted.</param>
    /// <returns>The string value of the provided <see cref="ErrorCode" />.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="value" /> is null. The conversion is implicit, so this can
    /// fire on a line that names no cast.
    /// </exception>
    public static implicit operator string(ErrorCode value) =>
        value is null
            ? throw new ArgumentNullException(nameof(value))
            : value.Value;

    /// <summary>
    /// Implicitly converts a string value to an <see cref="ErrorCode" />
    /// instance.
    /// </summary>
    /// <param name="value">
    /// The string value to be converted to an
    /// <see cref="ErrorCode" /> instance.
    /// </param>
    /// <returns>
    /// A new <see cref="ErrorCode" /> instance created from the provided
    /// string value.
    /// </returns>
    public static implicit operator ErrorCode(string value) => new(value);

    /// <summary>Returns the error code string value.</summary>
    /// <returns>The <see cref="Value" /> of this error code.</returns>
    public override string ToString() => Value;
}
