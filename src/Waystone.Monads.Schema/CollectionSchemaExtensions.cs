namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;

/// <summary>Size rules for a schema producing a list or a dictionary.</summary>
/// <remarks>
/// <para>
/// Two overloads apiece rather than one rule over <c>IReadOnlyCollection&lt;T&gt;</c>,
/// because <see cref="Schema{TIn,TOut}" /> is invariant in its output: a
/// <c>Schema&lt;TIn, IReadOnlyList&lt;T&gt;&gt;</c> is not a
/// <c>Schema&lt;TIn, IReadOnlyCollection&lt;T&gt;&gt;</c>, so a rule written against
/// the common base is never a candidate for either receiver. Writing it generically
/// over the collection type instead compiles and then cannot be called fluently,
/// because the element type would appear only in a constraint and C# infers no type
/// argument from one.
/// </para>
/// <para>
/// All four report <c>schema_violation.out-of-range</c> and supply the bound to
/// <c>{Expected}</c>. They count entries, not their contents, and they run
/// regardless of whether the entries themselves passed — so a list of three bad
/// entries with a minimum of five reports four failures, not one.
/// </para>
/// </remarks>
public static class CollectionSchemaExtensions
{
    private const string TooFew =
        "Expected {Path} to hold at least {Expected} entries.";

    private const string TooMany =
        "Expected {Path} to hold at most {Expected} entries.";

    /// <summary>Requires a list to hold no fewer entries than a bound.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <typeparam name="T">The type of a parsed entry.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="count">
    /// The fewest entries accepted. Inclusive. Pass 1 to reject an empty list,
    /// which is the common case and is not the same as the field being absent —
    /// <c>Schema.Required</c> reports that.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, IReadOnlyList<T>> MinCount<TIn, T>(
        this Schema<TIn, IReadOnlyList<T>> schema,
        int count) where TIn : notnull where T : notnull =>
        Rules.Add(
            schema,
            entries => entries.Count >= count,
            ViolationCode.OutOfRange,
            TooFew,
            count);

    /// <summary>Requires a list to hold no more entries than a bound.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <typeparam name="T">The type of a parsed entry.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="count">The most entries accepted. Inclusive.</param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <remarks>
    /// Worth setting on anything that arrives from outside. Every entry is parsed
    /// before this rule runs, so an unbounded list is an unbounded amount of work
    /// on behalf of whoever sent it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, IReadOnlyList<T>> MaxCount<TIn, T>(
        this Schema<TIn, IReadOnlyList<T>> schema,
        int count) where TIn : notnull where T : notnull =>
        Rules.Add(
            schema,
            entries => entries.Count <= count,
            ViolationCode.OutOfRange,
            TooMany,
            count);

    /// <summary>Requires a dictionary to hold no fewer entries than a bound.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <typeparam name="TKey">The type of a parsed key.</typeparam>
    /// <typeparam name="TValue">The type of a parsed value.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="count">The fewest entries accepted. Inclusive.</param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <remarks>
    /// Counts what came out, not what went in. Two keys that parsed to the same key
    /// are already a <c>schema_violation.duplicate</c> and produce no dictionary at
    /// all, so this rule never sees a collapsed count.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, IReadOnlyDictionary<TKey, TValue>>
        MinCount<TIn, TKey, TValue>(
            this Schema<TIn, IReadOnlyDictionary<TKey, TValue>> schema,
            int count)
        where TIn : notnull where TKey : notnull where TValue : notnull =>
        Rules.Add(
            schema,
            entries => entries.Count >= count,
            ViolationCode.OutOfRange,
            TooFew,
            count);

    /// <summary>Requires a dictionary to hold no more entries than a bound.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <typeparam name="TKey">The type of a parsed key.</typeparam>
    /// <typeparam name="TValue">The type of a parsed value.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="count">The most entries accepted. Inclusive.</param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, IReadOnlyDictionary<TKey, TValue>>
        MaxCount<TIn, TKey, TValue>(
            this Schema<TIn, IReadOnlyDictionary<TKey, TValue>> schema,
            int count)
        where TIn : notnull where TKey : notnull where TValue : notnull =>
        Rules.Add(
            schema,
            entries => entries.Count <= count,
            ViolationCode.OutOfRange,
            TooMany,
            count);
}
