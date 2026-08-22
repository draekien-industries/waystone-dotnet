namespace Waystone.Monads.Configs;

using System;
using Results.Errors;
#if !DEBUG
using System.Diagnostics;
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
    /// <remarks>
    /// Overriding this to shape your codes has been replaced by the declarative
    /// format on
    /// <see cref="Results.Errors.ErrorCodeProviderAttribute.Format" />, or on
    /// <see cref="Results.Errors.ErrorCodeFormatAttribute" /> for a whole assembly.
    /// A format is read at compile time, so the generated constants, the analyzers
    /// and the error code registry all agree on what an enum produces; an override
    /// here runs too late for any of them to see.
    /// </remarks>
    /// <param name="enum">The enum value to convert into an Error Code.</param>
    /// <returns>The created <see cref="ErrorCode" />.</returns>
    [Obsolete(
        "Shape error codes with [ErrorCodeProvider(Format = \"...\")] on the enum, or [assembly: ErrorCodeFormat(\"...\")] for every enum in the assembly. This member will be removed in 7.0.0.")]
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
