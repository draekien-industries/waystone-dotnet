namespace Waystone.Monads.Schemas;

using System;

/// <summary>Rules that only make sense on a schema which changes no type.</summary>
/// <remarks>
/// Extension methods rather than members of <see cref="Schema{TIn,TOut}" />,
/// because each of them can skip the schema it is called on, and skipping is only
/// well typed when the schema hands back what it was given. A member could not
/// say that: it would have to invent a <c>TOut</c> out of a <c>TIn</c> it never
/// parsed. So <c>When</c> is offered on <c>Schema&lt;T, T&gt;</c> and withheld
/// from <c>Schema&lt;string, EmailAddress&gt;</c>, which is a missing-method error
/// rather than a runtime one. Gate a transforming schema at the field instead, by
/// choosing between two fields in the <c>Configure</c> body.
/// </remarks>
public static class SchemaExtensions
{
    /// <summary>Applies a schema only to inputs that satisfy a condition.</summary>
    /// <typeparam name="T">The type the schema both accepts and produces.</typeparam>
    /// <param name="schema">The rules to apply conditionally.</param>
    /// <param name="predicate">
    /// The condition, read from the input before any rule runs. Returning false
    /// passes the input straight through unexamined.
    /// </param>
    /// <returns>A schema that either applies the rules or passes the input on.</returns>
    /// <remarks>
    /// Gates the whole receiver, not the last rule added to it. To gate one rule
    /// out of several, build that rule on its own and combine:
    /// <code>
    /// Schema.All(Always, Sometimes.When(x => x.Length > 0))
    /// </code>
    /// A skipped schema reports nothing, so the input reaches the constructed
    /// object unchecked. That is the point, and it is also the risk — a condition
    /// that is accidentally never true turns a rule off in silence.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="predicate" /> is null.
    /// </exception>
    public static Schema<T, T> When<T>(
        this Schema<T, T> schema,
        Func<T, bool> predicate) where T : notnull =>
        new ConditionalSchema<T>(schema, predicate, true);

    /// <summary>Applies a schema except to inputs that satisfy a condition.</summary>
    /// <typeparam name="T">The type the schema both accepts and produces.</typeparam>
    /// <param name="schema">The rules to apply conditionally.</param>
    /// <param name="predicate">
    /// The condition, read from the input before any rule runs. Returning true
    /// passes the input straight through unexamined.
    /// </param>
    /// <returns>A schema that either applies the rules or passes the input on.</returns>
    /// <remarks>
    /// The negation of <see cref="When{T}" />, and there only so a condition can be
    /// written the way it reads aloud. Prefer whichever spelling leaves the
    /// predicate free of <c>!</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="predicate" /> is null.
    /// </exception>
    public static Schema<T, T> Unless<T>(
        this Schema<T, T> schema,
        Func<T, bool> predicate) where T : notnull =>
        new ConditionalSchema<T>(schema, predicate, false);
}
