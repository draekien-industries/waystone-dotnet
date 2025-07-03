namespace Waystone.Monads.Results.Errors;

using System;
using Waystone.Monads.Configs;

/// <summary>
/// Represents an error that contains both an error code and a descriptive
/// message.
/// </summary>
/// <remarks>
/// This class can be used to provide detailed information about errors in
/// an application. It encapsulates an <see cref="ErrorCode" /> and an accompanying
/// error message. Additionally, it provides constructors for creating error
/// instances from exceptions or directly passing error codes and messages.
/// </remarks>
public record Error
{
    /// <summary>
    /// Creates a new instance of <see cref="Error" /> from an
    /// <see cref="ErrorCode" /> and a message string.
    /// </summary>
    /// <param name="code">
    /// The <see cref="ErrorCode" /> that uniquely identifies the
    /// type of error.
    /// </param>
    /// <param name="message">
    /// A descriptive error message providing more context about
    /// the error.
    /// </param>
    public Error(ErrorCode code, string message)
    {
        Code = code;
        Message = string.IsNullOrWhiteSpace(message)
            ? MonadOptions.Global.FallbackErrorMessage
            : message.Trim();
    }

    /// <summary>
    /// The <see cref="ErrorCode" /> that uniquely identifies the type of
    /// error.
    /// </summary>
    public ErrorCode Code { get; }

    /// <summary>A descriptive error message providing more context about the error.</summary>
    public string Message { get; }

    /// <summary>
    /// Creates a new instance of <see cref="Error" /> from an exception.
    /// </summary>
    /// <remarks>
    /// Uses the <see cref="ErrorCodeFactory"/> configured in
    /// <see cref="MonadOptions"/> to create the error code.
    /// </remarks>
    /// <param name="exception">The exception.</param>
    /// <returns>The created <see cref="Error" /></returns>
    public static Error FromException(Exception exception) => new(ErrorCode.FromException(exception), exception.Message);

    /// <inheritdoc />
    public override string ToString() => $"[{Code}] {Message}";
}
