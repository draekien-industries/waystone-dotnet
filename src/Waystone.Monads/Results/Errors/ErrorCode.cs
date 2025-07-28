namespace Waystone.Monads.Results.Errors;

using System;
using Configs;

/// <summary>A short code representing an error type in the application.</summary>
/// <remarks>
/// Error codes should not change between occurrence to occurrence of the
/// same error type, except for purposes of localization.
/// </remarks>
public record ErrorCode
{
    /// <summary>
    /// Creates a new instance of <see cref="ErrorCode" /> from a string
    /// value.
    /// </summary>
    /// <param name="value">The error code string value</param>
    public ErrorCode(string value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? MonadOptions.Global.FallbackErrorCode
            : value.Trim();
    }

    /// <summary>The error code string value</summary>
    public string Value { get; }

    /// <summary>Creates an instance of an <see cref="ErrorCode" /> from an enum value.</summary>
    /// <remarks>
    /// Uses the <see cref="ErrorCodeFactory" /> configured in
    /// <see cref="MonadOptions" />.
    /// </remarks>
    /// <param name="value">The enum value to create the error code from.</param>
    /// <returns>The created instance of <see cref="ErrorCode" />.</returns>
    public static ErrorCode FromEnum(Enum value) =>
        MonadOptions.Global.ErrorCodeFactory.FromEnum(value);

    /// <summary>
    /// (Not Recommended) Creates an instance of an <see cref="ErrorCode" />
    /// from an exception.
    /// </summary>
    /// <remarks>
    /// Uses the <see cref="ErrorCodeFactory" /> configured in
    /// <see cref="MonadOptions" />.
    /// </remarks>
    /// <param name="exception"></param>
    /// <returns>The created instance of <see cref="ErrorCode" />.</returns>
    public static ErrorCode FromException(Exception exception) =>
        MonadOptions.Global.ErrorCodeFactory.FromException(exception);

    /// <summary>
    /// Implicitly converts an <see cref="ErrorCode" /> instance to its string
    /// representation.
    /// </summary>
    /// <param name="value">The <see cref="ErrorCode" /> instance to be converted.</param>
    /// <returns>The string value of the provided <see cref="ErrorCode" />.</returns>
    public static implicit operator string(ErrorCode value) => value.ToString();

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

    /// <inheritdoc />
    public override string ToString() => Value;
}
