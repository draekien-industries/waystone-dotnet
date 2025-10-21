namespace Waystone.Monads.Configs;

#if !DEBUG
using System;
using System.Diagnostics;
using Results.Errors;
#endif

/// <summary>
/// A factory for creating <see cref="ErrorCode" /> instances from enums
/// and exceptions.
/// </summary>
#if !DEBUG
[DebuggerStepThrough]
#endif
public class ErrorCodeFactory
{
    private const string NameOfException = nameof(Exception);

    /// <summary>Creates a new instance of <see cref="ErrorCode" /> from an Enum value.</summary>
    /// <param name="enum">The enum value to convert into an Error Code.</param>
    /// <returns>The created <see cref="ErrorCode" />.</returns>
    public virtual ErrorCode FromEnum(Enum @enum)
    {
        Type enumType = @enum.GetType();
        return new ErrorCode($"{enumType.Name}.{@enum}");
    }

    /// <summary>
    /// Creates a new instance of <see cref="ErrorCode" /> from an Exception
    /// value.
    /// </summary>
    /// <param name="exception">The exception value to convert into an Error Code.</param>
    /// <returns>The created <see cref="ErrorCode" />.</returns>
    public virtual ErrorCode FromException(Exception exception)
    {
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
