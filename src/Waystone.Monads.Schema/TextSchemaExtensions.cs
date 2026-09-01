namespace Waystone.Monads.Schemas;

using System;
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
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(1);

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

    /// <summary>Requires the value to match a regular expression.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="pattern">
    /// The expression, which has to be found <i>somewhere</i> in the value unless
    /// you anchor it with <c>^</c> and <c>$</c>. Compiled once when the schema is
    /// built, not once per parse, and reaches the message through
    /// <c>{Expected}</c>.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// Matching is given one second, after which it throws rather than blocking the
    /// caller. That is a guard against a pattern whose cost explodes on a crafted
    /// input, which is a live risk here because the pattern is yours and the value
    /// is not. Pass a <see cref="Regex" /> you built yourself to choose a different
    /// limit, or to use a source-generated one.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="pattern" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// If <paramref name="pattern" /> is not a valid regular expression.
    /// </exception>
    /// <exception cref="RegexMatchTimeoutException">
    /// Thrown from the parse, not from here, if matching a value takes longer than
    /// a second.
    /// </exception>
    public static Schema<TIn, string> Matches<TIn>(
        this Schema<TIn, string> schema,
        string pattern) where TIn : notnull
    {
        if (pattern is null) throw new ArgumentNullException(nameof(pattern));

        return Matches(
            schema,
            new Regex(pattern, RegexOptions.None, MatchTimeout));
    }

    /// <summary>Requires the value to match a regular expression you built.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <param name="pattern">
    /// The expression. Take this overload to set <see cref="RegexOptions" />, to
    /// choose your own match timeout, or to pass a source-generated
    /// <see cref="Regex" />; the string overload fixes all three.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// <see cref="Regex" /> is thread-safe for matching, so one instance is safely
    /// shared by every parse this schema runs. Give it a match timeout — an
    /// expression built with none will run against an untrusted value for as long
    /// as it takes.
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
}
