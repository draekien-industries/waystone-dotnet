namespace Waystone.Monads.Schemas;

using System;

/// <summary>Rules for a schema producing a UUID.</summary>
public static class UuidSchemaExtensions
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

    /// <summary>Requires the UUID to have been generated at random.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// <para>
    /// Version 4 is what <see cref="Guid.NewGuid" /> produces, so this is the rule
    /// for an identifier that should carry no information — no timestamp a reader
    /// could mine, and no ordering an attacker could walk.
    /// </para>
    /// <para>
    /// Rejects <see cref="Guid.Empty" /> as a side effect, since its version digits
    /// are zero. Chaining <c>NotEmpty</c> as well is harmless but says nothing new.
    /// </para>
    /// <para>
    /// Reads the version digits and nothing else. A value with them set to 4 passes
    /// even if the remaining bits were not random, because nothing in a UUID records
    /// how it was really made.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, Guid> IsVersion4<TIn>(
        this Schema<TIn, Guid> schema) where TIn : notnull =>
        WithVersion(schema, 4);

#if NET9_0_OR_GREATER
    /// <summary>Requires the UUID to carry the time it was created.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// <para>
    /// Version 7 leads with a millisecond timestamp, so a run of them sorts by
    /// creation order. That is what makes it the right key for a database index, and
    /// what makes it the wrong choice where the creation time is a secret.
    /// </para>
    /// <para>
    /// Only on .NET 9 and later, which is where <see cref="Guid.CreateVersion7()" />
    /// arrived. A consumer on an earlier framework cannot produce one of these, so
    /// the package does not offer to check for one.
    /// </para>
    /// <para>
    /// Rejects <see cref="Guid.Empty" />, and reads the version digits alone. It does
    /// not check that the embedded timestamp is plausible; bound that with
    /// <c>Transform</c> onto <c>Schema.Timestamp</c> if it matters.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, Guid> IsVersion7<TIn>(
        this Schema<TIn, Guid> schema) where TIn : notnull =>
        WithVersion(schema, 7);
#endif

    private static Schema<TIn, Guid> WithVersion<TIn>(
        Schema<TIn, Guid> schema,
        int version) where TIn : notnull =>
        Rules.Add(
            schema,
            value => VersionOf(value) == version,
            ViolationCode.Mismatched,
            "Expected {Path} to be a version {Expected} UUID.",
            version);

    private static int VersionOf(Guid value) => value.ToByteArray()[7] >> 4;
}
