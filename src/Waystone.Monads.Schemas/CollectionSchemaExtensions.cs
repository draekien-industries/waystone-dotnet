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
/// All six report <c>schema_violation.out-of-range</c> and supply the bound to
/// <c>{Expected}</c>, and none of them looks at what an entry holds.
/// </para>
/// <para>
/// The four that count what parsed run regardless of whether the entries passed,
/// so a list of three bad entries with a minimum of five reports four failures,
/// not one. The two <c>MaxCount</c> overloads that count the <i>input</i> are the
/// exception and stop the parse dead: nothing is parsed and nothing later in the
/// chain runs, so a collection past the bound reports the bound alone. That is
/// what makes them a guard rather than a report, and it is why they bind in
/// preference wherever both apply.
/// </para>
/// </remarks>
public static class CollectionSchemaExtensions
{
    private const string TooFew =
        "Expected {Path} to hold at least {Expected} entries.";

    internal const string TooMany =
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
    /// on behalf of whoever sent it. The overload taking a schema over a list
    /// counts first and parses nothing when the bound is broken; it is the one
    /// that binds for <c>Schema.List</c>, and the one to reach for on untrusted
    /// input.
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

    /// <summary>Requires a list to hold no more entries than a bound, counting first.</summary>
    /// <typeparam name="TIn">The type each entry arrives as.</typeparam>
    /// <typeparam name="T">The type of a parsed entry.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="count">The most entries accepted. Inclusive.</param>
    /// <returns>A schema that counts the input before parsing any of it.</returns>
    /// <remarks>
    /// Binds in preference to the overload above wherever the schema's input is
    /// the list being counted, which is every <c>Schema.List</c>. The count is
    /// knowable there without parsing, so a list past the bound costs one
    /// comparison rather than one parse per entry — the difference between a bound
    /// that protects a service and a bound that only reports afterwards.
    /// <para>
    /// The cost is the rest of the report. Breaking the bound produces no value, so
    /// the entries are never parsed and no rule after this one runs: a list of sixty
    /// entries with a maximum of fifty reports "at most 50 entries" and nothing
    /// about the entries. Put this last in a chain only if that is what you want.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<IReadOnlyList<TIn>, IReadOnlyList<T>>
        MaxCount<TIn, T>(
            this Schema<IReadOnlyList<TIn>, IReadOnlyList<T>> schema,
            int count) where TIn : notnull where T : notnull
    {
        if (schema is null) throw new ArgumentNullException(nameof(schema));

        return new InputCountSchema<IReadOnlyList<TIn>, IReadOnlyList<T>>(
            schema,
            static entries => entries.Count,
            count);
    }

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

    /// <summary>
    /// Requires a dictionary to hold no more entries than a bound, counting first.
    /// </summary>
    /// <typeparam name="TKeyIn">The type each key arrives as.</typeparam>
    /// <typeparam name="TValueIn">The type each value arrives as.</typeparam>
    /// <typeparam name="TKey">The type of a parsed key.</typeparam>
    /// <typeparam name="TValue">The type of a parsed value.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="count">The most entries accepted. Inclusive.</param>
    /// <returns>A schema that counts the input before parsing any of it.</returns>
    /// <remarks>
    /// The dictionary counterpart of the list overload above, and the one that
    /// binds for <c>Schema.Dictionary</c>. Counts the entries that arrived rather
    /// than the ones that parsed, which is the difference that lets it run first —
    /// and, as there, breaking the bound reports the bound alone, because nothing
    /// is parsed and nothing after it runs.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<IReadOnlyDictionary<TKeyIn, TValueIn>,
        IReadOnlyDictionary<TKey, TValue>> MaxCount<TKeyIn, TValueIn, TKey,
        TValue>(
        this Schema<IReadOnlyDictionary<TKeyIn, TValueIn>,
            IReadOnlyDictionary<TKey, TValue>> schema,
        int count)
        where TKeyIn : notnull
        where TValueIn : notnull
        where TKey : notnull
        where TValue : notnull
    {
        if (schema is null) throw new ArgumentNullException(nameof(schema));

        return new InputCountSchema<IReadOnlyDictionary<TKeyIn, TValueIn>,
            IReadOnlyDictionary<TKey, TValue>>(
            schema,
            static entries => entries.Count,
            count);
    }
}
