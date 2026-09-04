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
/// <para>
/// The rejected value is held until <see cref="Message" /> is read, so that a
/// schema which turns out to be nested inside a sensitive one can still redact
/// it. It is never handed out: <see cref="Path" />, <see cref="Code" /> and
/// <see cref="Message" /> are the whole surface. A caller wanting its own
/// wording branches on <see cref="Code" />.
/// </para>
/// </remarks>
public sealed record Violation
{
    private readonly ErrorCode _code;

    private readonly object? _expected;

    private readonly bool _isSensitive;

    private readonly ViolationPath _path;

    private readonly string? _predicate;

    private readonly object? _received;

    private readonly string _template;

    private string? _rendered;

    internal Violation(
        ViolationPath path,
        ErrorCode code,
        string template,
        object? received = null,
        object? expected = null,
        string? predicate = null,
        bool isSensitive = false)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _code = code ?? throw new ArgumentNullException(nameof(code));
        _template = template;
        _received = received;
        _expected = expected;
        _predicate = predicate;
        _isSensitive = isSensitive;
    }

    /// <summary>Gets the place in the parsed value this violation is about.</summary>
    public ViolationPath Path => _path;

    /// <summary>Gets the kind of failure, for a caller that branches rather than reads.</summary>
    /// <remarks>
    /// An <see cref="ErrorCode" /> rather than a <see cref="ViolationCode" />, so a
    /// schema is free to raise a code from its own domain. Compare against
    /// <c>ViolationCodeCatalog.Codes</c> to test for one of the built-in kinds:
    /// <code>
    /// if (violation.Code == ViolationCodeCatalog.Codes.Malformed) { }
    /// </code>
    /// </remarks>
    public ErrorCode Code => _code;

    /// <summary>Gets the failure described for a human reader.</summary>
    /// <remarks>
    /// Rendered on the first read rather than when the violation was created, so
    /// that <c>Sensitive</c> on a schema several levels up still reaches it. The
    /// text is fixed from then on. A sensitive schema renders the rejected value
    /// as <c>***</c>; every other schema renders it in full, so treat this text as
    /// carrying whatever the input carried.
    /// </remarks>
    public string Message => _rendered ??= Render();

    /// <summary>Checks whether another violation reports the same thing.</summary>
    /// <remarks>
    /// Compares the path, the code and the rendered message. The template and the
    /// rejected value behind that message take no part: two violations that read
    /// identically to a caller are the same violation, however each was built.
    /// </remarks>
    /// <param name="other">The violation to compare against. Null is never equal.</param>
    /// <returns>
    /// True if both report the same failure at the same place in the same words;
    /// false otherwise.
    /// </returns>
    public bool Equals(Violation? other) =>
        other is not null
     && Path.Equals(other.Path)
     && Code.Equals(other.Code)
     && string.Equals(Message, other.Message, StringComparison.Ordinal);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = Path.GetHashCode();

            hash = (hash * 397) ^ Code.GetHashCode();

            return (hash * 397)
                 ^ StringComparer.Ordinal.GetHashCode(Message);
        }
    }

    internal Violation Nested(ParseContext context) =>
        Moved(
            context.Path.Nest(Path),
            Code,
            _isSensitive || context.IsSensitive);

    internal Violation Retold(
        string template,
        object? received,
        bool isSensitive) =>
        new(
            Path,
            Code,
            template,
            received,
            null,
            null,
            _isSensitive || isSensitive);

    internal Violation Recoded(ErrorCode code) =>
        Moved(Path, code, _isSensitive);

    private Violation Moved(
        ViolationPath path,
        ErrorCode code,
        bool isSensitive) =>
        new(
            path,
            code,
            _template,
            _received,
            _expected,
            _predicate,
            isSensitive);

    private string Render() =>
        MessageTemplate.Render(
            _template,
            Path,
            Code,
            _received,
            _expected,
            _predicate,
            _isSensitive);
}
