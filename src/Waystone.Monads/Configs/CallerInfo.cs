namespace Waystone.Monads.Configs;

/// <summary>Represents information about the caller of a Try method.</summary>
/// <param name="MemberName">The caller member name</param>
/// <param name="ArgumentExpression">The caller argument expression</param>
/// <param name="LineNumber">The caller line number</param>
public record CallerInfo(
    string MemberName,
    string ArgumentExpression,
    int LineNumber);
