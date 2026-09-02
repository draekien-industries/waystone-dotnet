namespace Waystone.Monads.Schemas;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

/// <summary>Rules for a schema producing text.</summary>
/// <remarks>
/// Ordering matters more here than elsewhere, because <c>Trim</c> changes the
/// value the rules after it see. <c>Schema.Text.Trim().NotEmpty()</c> rejects a
/// string of spaces; <c>Schema.Text.NotEmpty().Trim()</c> accepts it and then
/// hands an empty string to the constructed object. Put <c>Trim</c> first.
/// </remarks>
public static class TextSchemaExtensions
{
    /// <summary>Removes the whitespace from both ends of the value.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose value to trim.</param>
    /// <returns>A schema producing the trimmed value.</returns>
    /// <remarks>
    /// A transform rather than a rule, so it reports nothing and cannot fail — but
    /// it does change what every later rule sees, and what the constructed object
    /// receives. Put it first on the chain, before the rules that judge the value.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, string> Trim<TIn>(this Schema<TIn, string> schema)
        where TIn : notnull
    {
        if (schema is null) throw new ArgumentNullException(nameof(schema));

        return schema.Transform(static value => value.Trim());
    }

    /// <summary>Requires the value to hold at least one character.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// Rejects only the empty string. A string of spaces has characters and passes,
    /// which is nearly never what a caller means — chain <c>Trim</c> first, and it
    /// is. Reports <c>schema_violation.out-of-range</c> rather than
    /// <c>incomplete</c>, because a value did arrive; <c>incomplete</c> is what
    /// <c>Schema.Required</c> reports when none did.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, string> NotEmpty<TIn>(
        this Schema<TIn, string> schema) where TIn : notnull =>
        Rules.Add(
            schema,
            static value => value.Length > 0,
            ViolationCode.OutOfRange,
            "Expected {Path} not to be empty.");

    /// <summary>Requires the value to be no shorter than a length.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="length">
    /// The fewest characters accepted. Inclusive. Counted in UTF-16 code units, so
    /// an emoji or another character outside the basic plane counts as two — do not
    /// use this to bound what a reader would call letters.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// The message names the length but not the value, since echoing a long string
    /// back into a report helps nobody. Default: <c>Expected {Path} to be at least
    /// {Expected} characters.</c>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, string> MinLength<TIn>(
        this Schema<TIn, string> schema,
        int length) where TIn : notnull =>
        Rules.Add(
            schema,
            value => value.Length >= length,
            ViolationCode.OutOfRange,
            "Expected {Path} to be at least {Expected} characters.",
            length);

    /// <summary>Requires the value to be no longer than a length.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="length">
    /// The most characters accepted. Inclusive. Counted in UTF-16 code units, so
    /// set it from the column width of the store you are writing to rather than
    /// from what a reader would count.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// Default message: <c>Expected {Path} to be at most {Expected} characters.</c>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, string> MaxLength<TIn>(
        this Schema<TIn, string> schema,
        int length) where TIn : notnull =>
        Rules.Add(
            schema,
            value => value.Length <= length,
            ViolationCode.OutOfRange,
            "Expected {Path} to be at most {Expected} characters.",
            length);

    /// <summary>Requires the value to match a regular expression you built.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="pattern">
    /// The expression, which has to be found <i>somewhere</i> in the value unless
    /// you anchor it with <c>^</c> and <c>$</c>. Reaches the message through
    /// <c>{Expected}</c>.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// <para>
    /// There is deliberately no overload taking a pattern string. Building the
    /// <see cref="Regex" /> yourself is what puts the choice of
    /// <see cref="RegexOptions" /> and of a match timeout in front of you, and it
    /// is what lets the compiler point you at <c>[GeneratedRegex]</c>. A string
    /// overload would have made the interpreted, uncached spelling the shortest
    /// one to write.
    /// </para>
    /// <para>
    /// <b>Give it a match timeout.</b> An expression built with none runs against
    /// an untrusted value for as long as it takes, and the pattern is yours while
    /// the value is not — which is the shape of a denial of service. One second is
    /// a reasonable ceiling. A timeout surfaces as
    /// <see cref="RegexMatchTimeoutException" /> from the parse, not from here.
    /// </para>
    /// <para>
    /// <see cref="Regex" /> is thread-safe for matching, so one instance is safely
    /// shared by every parse this schema runs. Build it once, in a static field.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="pattern" /> is null.
    /// </exception>
    public static Schema<TIn, string> Matches<TIn>(
        this Schema<TIn, string> schema,
        Regex pattern) where TIn : notnull
    {
        if (pattern is null) throw new ArgumentNullException(nameof(pattern));

        return Rules.Add(
            schema,
            pattern.IsMatch,
            ViolationCode.Mismatched,
            "Expected {Path} to match {Expected}, but got {Received}.",
            pattern);
    }

    /// <summary>Requires the value to look like an email address.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// <para>
    /// Checked by a single left-to-right scan rather than an expression, so there
    /// is no <c>Matches</c> timeout to trip and no cost a crafted input can blow
    /// up. The subset accepted is deliberately narrower than RFC 5322: a
    /// dot-separated local part of the unreserved and common punctuation
    /// characters, an <c>@</c>, then a host of letters, digits, dots and hyphens.
    /// </para>
    /// <para>
    /// It rejects what the RFC allows and nobody wants — quoted local parts,
    /// comments, bracketed IP literals, and any character outside ASCII. It accepts
    /// one thing a reader may not expect: a single-label host, so
    /// <c>root@localhost</c> passes. That is a real address on an internal network,
    /// and rejecting it here would leave no way to accept one.
    /// </para>
    /// <para>
    /// Passing says the value is <i>shaped</i> like an address, never that it
    /// exists or that anyone reads it. Send a message to learn that. Default
    /// message: <c>Expected {Path} to be an email address, but got {Received}.</c>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, string> Email<TIn>(
        this Schema<TIn, string> schema) where TIn : notnull =>
        Rules.Add(
            schema,
            static value => EmailAddress.IsWellFormed(value),
            ViolationCode.Malformed,
            "Expected {Path} to be an email address, but got {Received}.");

    /// <summary>Requires the value to be an absolute URL.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// <para>
    /// Accepts any scheme, so <c>file:</c>, <c>javascript:</c> and <c>data:</c> all
    /// pass. Take the overload naming the schemes you accept whenever the value
    /// will be followed, rendered into a page, or stored as a link — those three
    /// are how an open redirect and a script injection arrive. Default message:
    /// <c>Expected {Path} to be a URL, but got {Received}.</c>
    /// </para>
    /// <para>
    /// The value has to spell its scheme out, so <c>/quests/3</c> is rejected
    /// everywhere. <see cref="Uri" /> alone would accept a bare path as a
    /// <c>file:</c> URI on Unix and reject it on Windows, and a rule that decides
    /// differently per host is worse than one that decides wrongly.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, string> Url<TIn>(this Schema<TIn, string> schema)
        where TIn : notnull =>
        Rules.Add(
            schema,
            static value => IsAbsoluteUrl(value, out _),
            ViolationCode.Malformed,
            "Expected {Path} to be a URL, but got {Received}.");

    /// <summary>Requires the value to be an absolute URL of a scheme you allow.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="schemes">
    /// The schemes accepted, written without the colon — <c>"https"</c>, not
    /// <c>"https:"</c>. Matched without regard to case, since a scheme is
    /// case-insensitive and arrives in whichever case the caller typed.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// The overload to reach for by default. Passing no scheme accepts none and so
    /// fails every value: the rule will not quietly widen into the unrestricted one
    /// because an empty list reached it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="schemes" /> is null, or if
    /// any scheme in it is.
    /// </exception>
    public static Schema<TIn, string> Url<TIn>(
        this Schema<TIn, string> schema,
        params string[] schemes) where TIn : notnull
    {
        string[] accepted = Copy(schemes, nameof(schemes));

        return Rules.Add(
            schema,
            value => IsAbsoluteUrl(value, out Uri? parsed)
                  && Holds(
                         accepted,
                         parsed.Scheme,
                         StringComparison.OrdinalIgnoreCase),
            ViolationCode.Malformed,
            "Expected {Path} to be a {Expected} URL, but got {Received}.",
            string.Join(" or ", accepted));
    }

    /// <summary>Requires the value to be one of a fixed set.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="accepted">
    /// The values accepted, compared with <see cref="StringComparison.Ordinal" />.
    /// Take the overload carrying a comparison to ignore case.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// The set reaches the message through <c>{Expected}</c>, joined by commas, so
    /// a caller is told what they could have sent instead. Keep it short for that
    /// reason. A closed set the domain already models belongs in
    /// <c>Schema.Enum</c>, which yields the member rather than its spelling.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="accepted" /> is null, or if
    /// any value in it is.
    /// </exception>
    public static Schema<TIn, string> OneOf<TIn>(
        this Schema<TIn, string> schema,
        params string[] accepted) where TIn : notnull =>
        OneOf(schema, StringComparison.Ordinal, accepted);

    /// <summary>Requires the value to be one of a fixed set, compared how you say.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="comparison">
    /// How to compare. <see cref="StringComparison.OrdinalIgnoreCase" /> is the
    /// usual choice for something a person typed. A culture-sensitive comparison
    /// makes the rule accept different values on different machines, which is
    /// rarely what a fixed set means.
    /// </param>
    /// <param name="accepted">The values accepted.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="accepted" /> is null, or if
    /// any value in it is.
    /// </exception>
    public static Schema<TIn, string> OneOf<TIn>(
        this Schema<TIn, string> schema,
        StringComparison comparison,
        params string[] accepted) where TIn : notnull
    {
        string[] values = Copy(accepted, nameof(accepted));

        return Rules.Add(
            schema,
            value => Holds(values, value, comparison),
            ViolationCode.NotAllowed,
            "Expected {Path} to be one of {Expected}, but got {Received}.",
            string.Join(", ", values));
    }

    /// <summary>Requires the value to hold an exact number of characters.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="length">
    /// The number of characters accepted, counted in UTF-16 code units. The rule
    /// for a fixed-width code — a country code, the last four of a card — rather
    /// than for anything a person composes.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// Default message: <c>Expected {Path} to be exactly {Expected} characters.</c>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, string> Length<TIn>(
        this Schema<TIn, string> schema,
        int length) where TIn : notnull =>
        Rules.Add(
            schema,
            value => value.Length == length,
            ViolationCode.OutOfRange,
            "Expected {Path} to be exactly {Expected} characters.",
            length);

    /// <summary>Requires the value's length to fall within a range.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="min">The fewest characters accepted. Inclusive.</param>
    /// <param name="max">The most characters accepted. Inclusive.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// Prefer this to <c>MinLength(3).MaxLength(40)</c>. The two accept the same
    /// values, but the chain is two rules, so one mistake is reported twice.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// If <paramref name="max" /> is smaller than <paramref name="min" />, which
    /// describes a length no value can have. Thrown when the schema is built, so
    /// the mistake surfaces at startup rather than on a parse.
    /// </exception>
    public static Schema<TIn, string> LengthBetween<TIn>(
        this Schema<TIn, string> schema,
        int min,
        int max) where TIn : notnull
    {
        Rules.RequireOrdered(min, max, nameof(max));

        return Rules.Add(
            schema,
            value => value.Length >= min && value.Length <= max,
            ViolationCode.OutOfRange,
            "Expected {Path} to be between {Expected} characters.",
            min + " and " + max);
    }

    /// <summary>Requires the value to begin with a literal.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="prefix">The text the value has to begin with.</param>
    /// <param name="comparison">
    /// How to compare. Default: <see cref="StringComparison.Ordinal" />.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// A literal, not an expression. Reach for this over <c>Matches("^tag:")</c>:
    /// the characters are matched as themselves, so a dot or a bracket in the
    /// prefix means what it looks like rather than what a regular expression would
    /// make of it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="prefix" /> is null.
    /// </exception>
    public static Schema<TIn, string> StartsWith<TIn>(
        this Schema<TIn, string> schema,
        string prefix,
        StringComparison comparison = StringComparison.Ordinal)
        where TIn : notnull
    {
        if (prefix is null) throw new ArgumentNullException(nameof(prefix));

        return Rules.Add(
            schema,
            value => value.StartsWith(prefix, comparison),
            ViolationCode.Malformed,
            "Expected {Path} to start with {Expected}, but got {Received}.",
            prefix);
    }

    /// <summary>Requires the value to end with a literal.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="suffix">The text the value has to end with.</param>
    /// <param name="comparison">
    /// How to compare. Default: <see cref="StringComparison.Ordinal" />.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// A literal, not an expression, for the reason <c>StartsWith</c> gives.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="suffix" /> is null.
    /// </exception>
    public static Schema<TIn, string> EndsWith<TIn>(
        this Schema<TIn, string> schema,
        string suffix,
        StringComparison comparison = StringComparison.Ordinal)
        where TIn : notnull
    {
        if (suffix is null) throw new ArgumentNullException(nameof(suffix));

        return Rules.Add(
            schema,
            value => value.EndsWith(suffix, comparison),
            ViolationCode.Malformed,
            "Expected {Path} to end with {Expected}, but got {Received}.",
            suffix);
    }

    /// <summary>Requires the value to contain a literal.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="text">The text that has to appear somewhere in the value.</param>
    /// <param name="comparison">
    /// How to compare. Default: <see cref="StringComparison.Ordinal" />.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// Pair it with <c>Not</c> to reject a literal instead, which is the shape most
    /// deny-lists want: <c>Schema.Text.Not(Schema.Text.Contains("@"), "No
    /// addresses here.")</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="text" /> is null.
    /// </exception>
    public static Schema<TIn, string> Contains<TIn>(
        this Schema<TIn, string> schema,
        string text,
        StringComparison comparison = StringComparison.Ordinal)
        where TIn : notnull
    {
        if (text is null) throw new ArgumentNullException(nameof(text));

        return Rules.Add(
            schema,
            value => value.IndexOf(text, comparison) >= 0,
            ViolationCode.Malformed,
            "Expected {Path} to contain {Expected}, but got {Received}.",
            text);
    }

    private static bool IsAbsoluteUrl(
        string value,
        [NotNullWhen(true)] out Uri? parsed)
    {
        parsed = Uri.TryCreate(value, UriKind.Absolute, out Uri? candidate)
            ? candidate
            : null;

        if (parsed is null) return false;

        string scheme = parsed.Scheme;

        if (value.Length <= scheme.Length
         || value[scheme.Length] != ':'
         || !value.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
        {
            parsed = null;

            return false;
        }

        return true;
    }

    private static bool Holds(
        string[] values,
        string value,
        StringComparison comparison)
    {
        for (int index = 0; index < values.Length; index++)
        {
            if (string.Equals(value, values[index], comparison)) return true;
        }

        return false;
    }

    private static string[] Copy(string[] values, string parameter)
    {
        if (values is null) throw new ArgumentNullException(parameter);

        var copied = new string[values.Length];

        for (int index = 0; index < values.Length; index++)
        {
            copied[index] = values[index]
                         ?? throw new ArgumentNullException(parameter);
        }

        return copied;
    }
}
