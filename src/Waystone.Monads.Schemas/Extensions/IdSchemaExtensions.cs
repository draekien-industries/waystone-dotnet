namespace Waystone.Monads.Schemas;

using System;

/// <summary>Rules for a schema producing an identifier.</summary>
public static class IdSchemaExtensions
{
    /// <summary>Requires the identifier to have been set.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// <para>
    /// Rejects <see cref="Guid.Empty" /> and nothing else. Worth adding to almost
    /// every required identifier, because a <see cref="Guid" /> field the sender
    /// omitted deserialises to <see cref="Guid.Empty" /> rather than to null — so
    /// <c>Schema.Required</c> sees a value, reports nothing, and the empty
    /// identifier reaches the database.
    /// </para>
    /// <para>
    /// Reports <c>schema_violation.mismatched</c> rather than <c>incomplete</c>. A
    /// value did arrive; it is the wrong one.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, Guid> NotEmpty<TIn>(
        this Schema<TIn, Guid> schema) where TIn : notnull =>
        Rules.Add(
            schema,
            static value => value != Guid.Empty,
            ViolationCode.Mismatched,
            "Expected {Path} not to be an empty identifier.");
}
