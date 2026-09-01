namespace Waystone.Monads.Schemas;

using System;
using Waystone.Monads.Results.Errors;

/// <summary>Reports one reason a parse failed, and where.</summary>
/// <remarks>
/// <para>
/// A parse gathers rather than stops, so a failure normally carries several of
/// these. Reach them through <see cref="SchemaViolation.Violations" />.
/// </para>
/// <para>
/// Only a parse constructs one. There is no way to raise a violation from
/// outside the schema that produced it, which is what keeps
/// <see cref="Path" /> honest.
/// </para>
/// </remarks>
public sealed record Violation
{
    internal Violation(ViolationPath path, ErrorCode code, string message)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message;
    }

    /// <summary>Gets the place in the parsed value this violation is about.</summary>
    public ViolationPath Path { get; }

    /// <summary>Gets the kind of failure, for a caller that branches rather than reads.</summary>
    /// <remarks>
    /// An <see cref="ErrorCode" /> rather than a <see cref="ViolationCode" />, so a
    /// schema is free to raise a code from its own domain. Compare against
    /// <c>ViolationCodeCatalog.Codes</c> to test for one of the built-in kinds:
    /// <code>
    /// if (violation.Code == ViolationCodeCatalog.Codes.Malformed) { }
    /// </code>
    /// </remarks>
    public ErrorCode Code { get; }

    /// <summary>Gets the failure described for a human reader.</summary>
    /// <remarks>
    /// Rendered when the violation was created, so it reflects the message
    /// template and the ambient options in force during the parse rather than
    /// during the read. A schema marked sensitive renders the rejected value as
    /// <c>***</c> here; every other schema renders it in full, so treat this text
    /// as carrying whatever the input carried.
    /// </remarks>
    public string Message { get; }

    internal Violation Rebase(ViolationPath parent) =>
        new(parent.Nest(Path), Code, Message);
}
