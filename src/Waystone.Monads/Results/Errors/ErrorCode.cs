namespace Waystone.Monads.Results.Errors;

using System;
using Waystone.Monads.Configs;

/// <summary>A short code representing an error type in the application.</summary>
/// <example>
/// This shows how to create reusable error codes for your application.
/// <code>
/// public static class ErrorCodes
/// {
///     public static readonly ErrorCode EntityNotFound = new("Entity.NotFound");
///     public static readonly ErrorCode EntityConflict = new("Entity.Conflict");
/// }
/// </code>
/// </example>
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

    /// <summary>
    /// Creates an instance of an <see cref="ErrorCode" /> from an enum value.
    /// </summary>
    /// <remarks>
    /// Uses the <see cref="ErrorCodeFactory"/> configured in <see cref="MonadOptions"/>.
    /// </remarks>
    /// <param name="value">The enum value to create the error code from.</param>
    /// <returns>The created instance of <see cref="ErrorCode" />.</returns>
    public static ErrorCode FromEnum(Enum value) => MonadOptions.Global.ErrorCodeFactory.FromEnum(value);

    /// <summary>
    /// (Not Recommended) Creates an instance of an <see cref="ErrorCode" /> from an exception.
    /// </summary>
    /// <remarks>
    /// Uses the <see cref="ErrorCodeFactory"/> configured in <see cref="MonadOptions"/>.
    /// </remarks>
    /// <param name="exception"></param>
    /// <returns>The created instance of <see cref="ErrorCode" />.</returns>
    public static ErrorCode FromException(Exception exception) => MonadOptions.Global.ErrorCodeFactory.FromException(exception);

    /// <summary>
    /// Implicitly converts an <see cref="ErrorCode" /> instance to its string
    /// representation.
    /// </summary>
    /// <param name="value">The <see cref="ErrorCode" /> instance to be converted.</param>
    /// <returns>The string value of the provided <see cref="ErrorCode" />.</returns>
    public static implicit operator string(ErrorCode value) => value.ToString();

    /// <inheritdoc />
    public override string ToString() => Value;
}
