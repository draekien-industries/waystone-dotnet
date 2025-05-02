namespace Waystone.Monads.Results.Errors;

using System;

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
    private const string UnspecifiedMessage = "An unexpected error occurred.";

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
            ? UnspecifiedMessage
            : message.Trim();
    }

    /// <summary>
    /// The <see cref="ErrorCode" /> that uniquely identifies the type of
    /// error.
    /// </summary>
    public ErrorCode Code { get; }

    /// <summary>A descriptive error message providing more context about the error.</summary>
    public string Message { get; }

    /// <summary>Creates a new instance of <see cref="Error" /> from an exception</summary>
    /// <param name="exception">The exception</param>
    /// <param name="errorCodeFormatter">
    /// Optional. An
    /// <see cref="IErrorCodeFormatter{T}" /> that will be used to generate the error
    /// code
    /// </param>
    /// <typeparam name="TException">The exception instance type</typeparam>
    /// <returns>The created <see cref="Error" /></returns>
    public static Error FromException<TException>(
        TException exception,
        IErrorCodeFormatter<TException>? errorCodeFormatter = null)
        where TException : Exception => new(
        ErrorCode.FromException(exception, errorCodeFormatter),
        exception.Message);

    /// <inheritdoc />
    public override string ToString() => $"[{Code}] {Message}";
}
