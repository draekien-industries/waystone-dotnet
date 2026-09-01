namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;

public abstract partial class Schema
{
    /// <summary>Applies one schema to every entry of a list.</summary>
    /// <typeparam name="TIn">The type each entry arrives as.</typeparam>
    /// <typeparam name="TOut">The type each entry parses into.</typeparam>
    /// <param name="item">
    /// The schema to run against each entry, at that entry's own path. It sees one
    /// entry at a time and knows nothing of its neighbours, so a rule about the
    /// list as a whole belongs on the result of this call rather than inside it.
    /// </param>
    /// <returns>
    /// A schema over any <see cref="IReadOnlyList{T}" /> — an array and a
    /// <c>List&lt;T&gt;</c> both qualify — producing the parsed entries in the order
    /// they arrived.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Every entry runs, so one bad entry does not hide the next. Failures are
    /// reported at an indexed path, which is what makes a report usable against a
    /// form: an entry three deep in a list of order lines comes back as
    /// <c>lines[3].sku</c>, not as a message about <c>lines</c>.
    /// </para>
    /// <para>
    /// The list produces a value only when every entry does. An entry whose chain
    /// halts on a failed transform contributes none, so there is no complete list to
    /// hand on — but the surviving entries still report their own failures in full,
    /// and so do the fields beside this one. A null entry is reported as
    /// <c>schema_violation.incomplete</c> at its index and never reaches
    /// <paramref name="item" />, which is why an item schema may assume its input is
    /// there.
    /// </para>
    /// <para>
    /// Bound the length with <c>MinCount</c> and <c>MaxCount</c> on the result.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="item" /> is null.
    /// </exception>
    public static Schema<IReadOnlyList<TIn>, IReadOnlyList<TOut>> List<TIn, TOut>(
        Schema<TIn, TOut> item)
        where TIn : notnull where TOut : notnull =>
        new ListSchema<TIn, TOut>(item);

    /// <summary>Applies one schema to every key of a dictionary and another to every value.</summary>
    /// <typeparam name="TKeyIn">The type each key arrives as.</typeparam>
    /// <typeparam name="TValueIn">The type each value arrives as.</typeparam>
    /// <typeparam name="TKeyOut">The type each key parses into.</typeparam>
    /// <typeparam name="TValueOut">The type each value parses into.</typeparam>
    /// <param name="key">
    /// The schema for a key. Worth more than it looks: a dictionary arriving from
    /// JSON has string keys, so this is where a currency code or an identifier
    /// stops being text.
    /// </param>
    /// <param name="value">The schema for the value stored under a key.</param>
    /// <returns>
    /// A schema over any <see cref="IReadOnlyDictionary{TKey,TValue}" />, producing
    /// the parsed entries.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Failures are reported at a keyed path built from the <i>incoming</i> key's
    /// text — <c>rates["AUD"]</c> — because that is the only spelling the caller can
    /// match against what they sent. A key failure and a value failure therefore
    /// share a path, and the message is what tells them apart.
    /// </para>
    /// <para>
    /// Two keys that parse to the same output key are a
    /// <c>schema_violation.duplicate</c>, reported against the later of the two.
    /// Silently keeping one of them would drop data the caller sent, and the
    /// dictionary produces no value in that case.
    /// </para>
    /// <para>
    /// Entries are parsed in whatever order the dictionary enumerates, which is not
    /// specified. Do not write a rule that depends on the order violations come
    /// back in; group them with <c>ByPath</c> instead.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="key" /> or <paramref name="value" /> is null.
    /// </exception>
    public static Schema<IReadOnlyDictionary<TKeyIn, TValueIn>,
        IReadOnlyDictionary<TKeyOut, TValueOut>> Dictionary<TKeyIn, TValueIn,
        TKeyOut, TValueOut>(
        Schema<TKeyIn, TKeyOut> key,
        Schema<TValueIn, TValueOut> value)
        where TKeyIn : notnull
        where TValueIn : notnull
        where TKeyOut : notnull
        where TValueOut : notnull =>
        new DictionarySchema<TKeyIn, TValueIn, TKeyOut, TValueOut>(key, value);
}
