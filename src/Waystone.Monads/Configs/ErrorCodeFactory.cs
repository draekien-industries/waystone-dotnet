namespace Waystone.Monads.Configs;

using System;
using Waystone.Monads.Results.Errors;

/// <summary>
/// A factory for creating <see cref="ErrorCode"/> instances from enums and exceptions.
/// </summary>
public class ErrorCodeFactory
{
    private const string NameOfException = nameof(Exception);

    /// <summary>
    /// Creates a new instance of <see cref="ErrorCode"/> from an Enum value.
    /// </summary>
    /// <param name="enum">The enum value to convert into an Error Code.</param>
    /// <returns>The created <see cref="ErrorCode"/>.</returns>
    public virtual ErrorCode FromEnum(Enum @enum)
    {
        var enumType = @enum.GetType();
        return new ErrorCode($"{enumType.Name}.{@enum}");
    }

    /// <summary>
    /// Creates a new instance of <see cref="ErrorCode"/> from an Exception value.
    /// </summary>
    /// <param name="exception">The exception value to convert into an Error Code.</param>
    /// <returns>The created <see cref="ErrorCode"/>.</returns>
    public virtual ErrorCode FromException(Exception exception)
    {
        var exceptionType = exception.GetType();
        var exceptionName = exceptionType.Name;

        return exceptionName switch
        {
            NameOfException => new ErrorCode(NameOfException),
            var _ when exceptionName.EndsWith(
                NameOfException,
                StringComparison.OrdinalIgnoreCase) =>
              new ErrorCode(exceptionName[..^NameOfException.Length]),
            _ => new ErrorCode(exceptionName),
        };
    }
}