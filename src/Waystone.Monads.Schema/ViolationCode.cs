namespace Waystone.Monads.Schemas;

using Waystone.Monads.Results.Errors;

/// <summary>
/// Names the built-in kinds of failure a schema can report, so a rule that has no
/// domain code of its own still says something a caller can branch on.
/// </summary>
/// <remarks>
/// <para>
/// A vocabulary, not a closed set. <see cref="Violation.Code" /> is an
/// <see cref="ErrorCode" />, and every rule that takes a
/// <see cref="ViolationCode" /> also takes an <see cref="ErrorCode" />, so a
/// schema is free to raise <c>order.line_count_exceeded</c> instead. Reach for one
/// of these when the failure is generic enough that a domain code would only
/// restate the check.
/// </para>
/// <para>
/// Seven kinds cover the whole surface deliberately. A caller that needs finer
/// detail reads <see cref="Violation.Path" /> to learn where, and
/// <see cref="Violation.Message" /> to learn what a human should be told; the code
/// answers only what kind of thing went wrong, which is the part worth writing a
/// branch against.
/// </para>
/// <para>
/// The enum carries <see cref="ErrorCodeCatalogAttribute" />, so
/// <c>ViolationCodeCatalog</c> is generated beside it with an
/// <see cref="ErrorCode" /> per member under the <c>schema_violation.</c> prefix —
/// <c>NotAllowed</c> becomes <c>schema_violation.not-allowed</c>. That is the
/// bridge between this enum and <see cref="Violation.Code" />, and those strings
/// reach consumers, so renaming a member is a breaking change even though nothing
/// in the build says so.
/// </para>
/// </remarks>
[ErrorCodeCatalog(Format = "schema_violation.{member:kebab}")]
public enum ViolationCode
{
    /// <summary>A value the schema requires was absent.</summary>
    /// <remarks>
    /// Nothing arrived, so no other check ran against this path. Distinct from
    /// <see cref="Malformed" />, where a value did arrive and could not be read.
    /// </remarks>
    Incomplete,

    /// <summary>A value arrived but could not be read as the type the schema expects.</summary>
    /// <remarks>
    /// The failure of a conversion rather than of a rule — text that is not a
    /// number, or a number a factory refused. A failed conversion produces no
    /// value, so the checks after it on the same field do not run.
    /// </remarks>
    Malformed,

    /// <summary>A value arrived at a path where the schema permits none.</summary>
    /// <remarks>
    /// The mirror of <see cref="Incomplete" />: the value is readable and would be
    /// valid elsewhere, but this path forbids it outright.
    /// </remarks>
    NotAllowed,

    /// <summary>A value fell outside a bound the schema sets.</summary>
    /// <remarks>
    /// Covers every ordered comparison — a length, a count, a magnitude, an
    /// instant before or after another.
    /// </remarks>
    OutOfRange,

    /// <summary>A value failed a pattern or an equality the schema sets.</summary>
    /// <remarks>
    /// The unordered counterpart of <see cref="OutOfRange" />: the value is not
    /// too large or too small, it is the wrong shape. A regular expression, a
    /// permitted set, a confirmation field that does not match its original.
    /// </remarks>
    Mismatched,

    /// <summary>Two entries in a collection broke a uniqueness the schema requires.</summary>
    /// <remarks>
    /// Reported at the path of the later entry, so a caller highlighting the
    /// failure points at the one a user would remove.
    /// </remarks>
    Duplicate,

    /// <summary>Two values are each valid alone and contradict one another together.</summary>
    /// <remarks>
    /// The code a cross-field rule produces. Reported at the path of the rule
    /// rather than of either value, since neither one is individually at fault.
    /// </remarks>
    Conflicting,
}
