namespace Waystone.Monads.Results.Errors;

using System;

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
    private const string UnspecifiedValue = "Err.Unspecified";

    /// <summary>
    /// Creates a new instance of <see cref="ErrorCode" /> from a string
    /// value.
    /// </summary>
    /// <param name="value">The error code string value</param>
    public ErrorCode(string value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? UnspecifiedValue
            : value.Trim();
    }

    /// <summary>The error code string value</summary>
    public string Value { get; }

    /// <summary>
    /// Creates an instance of an <see cref="ErrorCode" /> from an enum value
    /// of type <typeparamref name="TEnum" />.
    /// </summary>
    /// <remarks>
    /// The default format for error codes created in this way is
    /// <c>$"{typeof(TEnum).Name}.{value}"</c>
    /// </remarks>
    /// <param name="value">The error code enum value.</param>
    /// <param name="formatter">
    /// Optional. A formatter for mapping the enum value to a
    /// string error code value.
    /// </param>
    /// <typeparam name="TEnum">The error code enum type.</typeparam>
    /// <returns>The created instance of <see cref="ErrorCode" />.</returns>
    public static ErrorCode FromEnum<TEnum>(
        TEnum value,
        IErrorCodeFormatter<TEnum>? formatter = null) where TEnum : Enum
    {
        formatter ??= new DefaultEnumErrorCodeFormatter<TEnum>();
        return new ErrorCode(formatter.Format(value));
    }

    internal static ErrorCode FromException<TException>(
        TException exception,
        IErrorCodeFormatter<TException>? formatter = null)
        where TException : Exception
    {
        formatter ??= new DefaultExceptionErrorCodeFormatter<TException>();
        return new ErrorCode(formatter.Format(exception));
    }

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
