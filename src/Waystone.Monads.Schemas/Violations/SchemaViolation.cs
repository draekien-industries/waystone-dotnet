namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using System.Linq;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>The <see cref="Error" /> a failed parse produces.</summary>
/// <remarks>
/// <para>
/// It is an <see cref="Error" /> rather than something convertible to one, so a
/// parse composes with every other step in a
/// <see cref="Result{TOk,TErr}" /> chain without a <c>MapErr</c> at the seam.
/// Recover the detail by pattern matching:
/// <code>
/// if (error is SchemaViolation violated)
/// {
///     return ValidationProblem(violated.ToDictionary());
/// }
/// </code>
/// </para>
/// <para>
/// <see cref="Error.Code" /> is <c>schema_violation</c> for every failed parse,
/// because it says only that a parse failed — matching the type is a better test
/// than matching the string. Branch on <see cref="ViolationCode" /> through
/// <see cref="Violations" /> to learn what kind of thing went wrong, or use
/// <see cref="ByCode" />.
/// </para>
/// <para>
/// <see cref="Error.Message" /> joins the violation messages, so it carries
/// whatever they carry — including a rejected value, unless the schema was marked
/// sensitive. Two instances are equal when their code and message match;
/// <see cref="Violations" /> takes no part in equality, since the message is
/// derived from it.
/// </para>
/// </remarks>
public sealed record SchemaViolation : Error
{
    internal const string ErrorCodeName = "schema_violation";

    internal SchemaViolation(ViolationCollection violations) : base(
        new ErrorCode(ErrorCodeName),
        string.Join("; ", violations.Select(violation => violation.Message)))
    {
        Violations = violations;
    }

    /// <summary>Gets every reason the parse failed, in the schema's declaration order.</summary>
    /// <remarks>
    /// Never empty. Immutable, and the same instance on every read.
    /// </remarks>
    public ViolationCollection Violations { get; }

    /// <inheritdoc cref="ViolationCollection.ByPath" />
    public IReadOnlyDictionary<string, IReadOnlyList<Violation>> ByPath() =>
        Violations.ByPath();

    /// <inheritdoc cref="ViolationCollection.ByCode" />
    public IReadOnlyDictionary<ErrorCode, IReadOnlyList<Violation>> ByCode() =>
        Violations.ByCode();

    /// <inheritdoc cref="ViolationCollection.ToDictionary" />
    public IDictionary<string, string[]> ToDictionary() => Violations.ToDictionary();

    /// <summary>
    /// Checks whether another <see cref="SchemaViolation" /> reports the same code
    /// and message.
    /// </summary>
    /// <remarks>
    /// <see cref="Violations" /> is deliberately excluded.
    /// <see cref="Error.Message" /> is joined from it, so comparing both would only
    /// add reference equality over a list and make two errors describing the same
    /// failures compare unequal.
    /// </remarks>
    /// <param name="other">The error to compare against. Null is never equal.</param>
    /// <returns>
    /// True if both errors are <see cref="SchemaViolation" /> and their
    /// <see cref="Error.Code" /> and <see cref="Error.Message" /> match; false
    /// otherwise.
    /// </returns>
    public bool Equals(SchemaViolation? other) => base.Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>Returns the failure rendered as <c>[code] message</c>.</summary>
    /// <remarks>
    /// Keeps <see cref="Error" />'s rendering rather than the one a record would
    /// otherwise synthesise, which would print every property including
    /// <see cref="Violations" />.
    /// </remarks>
    /// <returns>
    /// <see cref="Error.Code" /> in square brackets, a space, then the joined
    /// violation messages.
    /// </returns>
    public override string ToString() => base.ToString();
}
