namespace Waystone.Monads.Schemas;

using System;

/// <summary>Rules for a schema producing a flag.</summary>
/// <remarks>
/// Both rules report <c>schema_violation.not-allowed</c> rather than
/// <c>mismatched</c>: the value arrived and was understood, and what is wrong is
/// that this side of the flag is not accepted here.
/// </remarks>
public static class BoolSchemaExtensions
{
    /// <summary>Requires the flag to be set.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// The rule behind an acceptance: terms agreed to, a declaration made. A flag
    /// that may legitimately be either way needs no rule at all — leave it as
    /// <c>Schema.Bool</c>. Default message: <c>Expected {Path} to be
    /// accepted.</c>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, bool> IsTrue<TIn>(this Schema<TIn, bool> schema)
        where TIn : notnull =>
        Rules.Add(
            schema,
            static value => value,
            ViolationCode.NotAllowed,
            "Expected {Path} to be accepted.");

    /// <summary>Requires the flag to be clear.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// The rarer half, and worth a second look when you reach for it. A flag that
    /// has to be clear often reads better as the opposite flag that has to be set,
    /// and a caller cannot misread <c>Active</c> the way they can misread
    /// <c>NotSuspended</c>. Default message: <c>Expected {Path} not to be
    /// set.</c>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, bool> IsFalse<TIn>(this Schema<TIn, bool> schema)
        where TIn : notnull =>
        Rules.Add(
            schema,
            static value => !value,
            ViolationCode.NotAllowed,
            "Expected {Path} not to be set.");
}
