namespace Waystone.Monads.Results.Errors;

using System;
using Configs;

/// <summary>
/// Represents an error that contains both an error code and a descriptive
/// message.
/// </summary>
/// <remarks>
/// Two errors are equal when both their <see cref="Code" /> and their
/// <see cref="Message" /> match; the message comparison is ordinal and
/// case-sensitive, so two reports of the same failure compare unequal if the
/// text differs. Branch on <see cref="Code" />, never on <see cref="Message" />.
/// </remarks>
public record Error
{
    /// <summary>
    /// Creates a new instance of <see cref="Error" /> from an
    /// <see cref="ErrorCode" /> and a message string.
    /// </summary>
    /// <remarks>
    /// The two arguments are treated differently, deliberately. Surrounding
    /// whitespace is trimmed off the message, and a message that is null, empty or
    /// whitespace is replaced by the fallback configured through
    /// <see cref="MonadOptions.UseFallbackErrorMessage" />, so
    /// <see cref="Message" /> is never null or blank. Default fallback:
    /// <c>An unexpected error occurred.</c> Pass a real message; the fallback says
    /// nothing about what actually failed. A null <paramref name="code" /> throws,
    /// because consumers branch on <see cref="Code" /> and no fallback would be
    /// correct.
    /// </remarks>
    /// <param name="code">
    /// The <see cref="ErrorCode" /> that uniquely identifies the
    /// type of error.
    /// </param>
    /// <param name="message">
    /// A descriptive error message providing more context about
    /// the error. Blank is replaced by the configured fallback.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="code" /> is null.
    /// </exception>
    public Error(ErrorCode code, string message)
    {
        if (code is null) throw new ArgumentNullException(nameof(code));

        Code = code;
        Message = string.IsNullOrWhiteSpace(message)
            ? MonadOptions.Current.FallbackErrorMessage
            : message.Trim();
    }

    /// <summary>
    /// The <see cref="ErrorCode" /> that uniquely identifies the type of
    /// error.
    /// </summary>
    public ErrorCode Code { get; }

    /// <summary>A descriptive error message, trimmed and never null or blank.</summary>
    public string Message { get; }

    /// <summary>Creates a new instance of <see cref="Error" /> from an enum value.</summary>
    /// <remarks>
    /// The code is worked out by reflection at run time through the
    /// <see cref="ErrorCodeFactory" /> configured in <see cref="MonadOptions" />, so
    /// it cannot apply the format declared on the enum and nothing tells you when a
    /// rename changes the code. Mark the enum with <c>[ErrorCodeCatalog]</c> and use
    /// the generated <c>ToError(message)</c> extension instead.
    /// </remarks>
    /// <param name="value">The enum value to create the error code from.</param>
    /// <param name="message">
    /// A descriptive error message providing more context
    /// about the error. Blank is replaced by the configured fallback.
    /// </param>
    /// <returns>The created <see cref="Error" />.</returns>
    public static Error FromEnum(Enum value, string message) => new(
#pragma warning disable CS0618
        ErrorCode.FromEnum(value),
#pragma warning restore CS0618
        message);

    /// <summary>Creates a new instance of <see cref="Error" /> from an exception.</summary>
    /// <remarks>
    /// The code is the exception's type name with a trailing <c>Exception</c>
    /// removed, from the <see cref="ErrorCodeFactory" /> configured in
    /// <see cref="MonadOptions" />. The message is
    /// <see cref="Exception.Message" /> verbatim, so anything the exception's text
    /// exposes reaches whoever reads the error.
    /// </remarks>
    /// <param name="exception">The exception to take the code and message from.</param>
    /// <returns>The created <see cref="Error" />.</returns>
    public static Error FromException(Exception exception) => new(
        ErrorCode.FromException(exception),
        exception.Message);

    /// <summary>Returns the error rendered as <c>[code] message</c>.</summary>
    /// <returns>
    /// The <see cref="Code" /> in square brackets, a space, then the
    /// <see cref="Message" />.
    /// </returns>
    public override string ToString() => $"[{Code}] {Message}";
}
