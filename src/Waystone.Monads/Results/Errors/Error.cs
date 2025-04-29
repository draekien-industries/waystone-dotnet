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
/// <param name="Code">
/// The <see cref="ErrorCode" /> that uniquely identifies the
/// type of error.
/// </param>
/// <param name="Message">
/// A descriptive error message providing more context about
/// the error.
/// </param>
public record Error(ErrorCode Code, string Message)
{
    /// <summary>
    /// Creates a new instance of <see cref="Error" /> from an error code and
    /// message.
    /// </summary>
    /// <param name="code">The error code</param>
    /// <param name="message">The error message</param>
    public Error(string code, string message) :
        this(new ErrorCode(code), message)
    { }

    /// <summary>
    /// Creates a new instance of <see cref="Error" /> from an error code and
    /// an exception
    /// </summary>
    /// <param name="code">The <see cref="ErrorCode" /></param>
    /// <param name="exception">The exception</param>
    public Error(ErrorCode code, Exception exception) : this(
        code,
        exception.Message)
    { }

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
