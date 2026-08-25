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
    /// Produces <c>{EnumTypeName}.{MemberName}</c>, worked out by reflection at run
    /// time, so it cannot apply the format declared on the enum and nothing tells
    /// you when a rename changes the code. Shape codes with
    /// <see cref="Results.Errors.ErrorCodeCatalogAttribute.Format" />, or with
    /// <see cref="Results.Errors.ErrorCodeFormatAttribute" /> for a whole assembly.
    /// A format is read at compile time, so the generated constants, the analyzers
    /// and the error code registry all agree on what an enum produces; an override
    /// here runs too late for any of them to see.
    /// </remarks>
    /// <param name="enum">The enum value to convert into an Error Code.</param>
    /// <returns>The created <see cref="ErrorCode" />.</returns>
    [Obsolete(
        "Shape error codes with [ErrorCodeCatalog(Format = \"...\")] on the enum, or [assembly: ErrorCodeFormat(\"...\")] for every enum in the assembly. This member will be removed in 7.0.0.")]
    public virtual ErrorCode FromEnum(Enum @enum)
    {
        Type enumType = @enum.GetType();
        return new ErrorCode($"{enumType.Name}.{@enum}");
    }

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
