namespace Waystone.Monads.Configs;

using System;
using Results.Errors;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>
/// A factory for creating <see cref="ErrorCode" /> instances from exceptions.
/// </summary>
/// <remarks>
/// Override <see cref="FromException" /> to change the code an exception
/// produces, and install the subclass with
/// <see cref="MonadOptions.UseErrorCodeFactory" />. Enum codes do not come
/// through here: they are settled at compile time by
/// <see cref="Results.Errors.ErrorCodeCatalogAttribute" />, so a factory cannot
/// change one.
/// </remarks>
#if !DEBUG
[DebuggerStepThrough]
#endif
public class ErrorCodeFactory
{
    private const string NameOfException = nameof(Exception);

    /// <summary>
    /// Creates a new instance of <see cref="ErrorCode" /> from an Exception
    /// value.
    /// </summary>
    /// <remarks>
    /// Produces the exception's type name with a trailing <c>Exception</c>
    /// removed, matched without regard to case —
    /// <see cref="InvalidOperationException" /> gives <c>InvalidOperation</c>.
    /// <see cref="Exception" /> itself is left whole rather than reduced to an
    /// empty code. The exception's message is never read, so nothing from its text
    /// reaches the code.
    /// </remarks>
    /// <param name="exception">The exception value to convert into an Error Code.</param>
    /// <returns>The created <see cref="ErrorCode" />.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="exception" /> is null.
    /// </exception>
    public virtual ErrorCode FromException(Exception exception)
    {
        if (exception is null) throw new ArgumentNullException(nameof(exception));

        Type exceptionType = exception.GetType();
        string exceptionName = exceptionType.Name;

        return exceptionName switch
        {
            NameOfException => new ErrorCode(NameOfException),
            var _ when exceptionName.EndsWith(
                    NameOfException,
                    StringComparison.OrdinalIgnoreCase) =>
                new ErrorCode(exceptionName[..^NameOfException.Length]),
            var _ => new ErrorCode(exceptionName),
        };
    }
}
