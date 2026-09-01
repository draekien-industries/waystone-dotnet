namespace Waystone.Monads.Schemas;

using System;

/// <summary>Sign rules for a schema producing one of the built-in numeric types.</summary>
/// <remarks>
/// <para>
/// Four overloads apiece rather than one generic rule, because a generic rule
/// needs a zero of its own type and <c>netstandard2.0</c> has no numeric
/// constraint to get one from. The bounds — <c>AtLeast</c>, <c>AtMost</c>,
/// <c>GreaterThan</c>, <c>LessThan</c> — are generic and cover every ordered type;
/// these two are here only because <c>Positive()</c> says what
/// <c>GreaterThan(0)</c> means, and reports it in those words.
/// </para>
/// <para>
/// Both are strict: zero is neither positive nor negative and fails each of them.
/// Accept zero with <c>AtLeast(0)</c> or <c>AtMost(0)</c>.
/// </para>
/// </remarks>
public static class NumberSchemaExtensions
{
    private const string ExpectedPositive =
        "Expected {Path} to be positive, but got {Received}.";

    private const string ExpectedNegative =
        "Expected {Path} to be negative, but got {Received}.";

    /// <summary>Requires a 32-bit integer to be greater than zero.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, int> Positive<TIn>(this Schema<TIn, int> schema)
        where TIn : notnull =>
        Rules.Add(
            schema,
            static value => value > 0,
            ViolationCode.OutOfRange,
            ExpectedPositive);

    /// <summary>Requires a 64-bit integer to be greater than zero.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, long> Positive<TIn>(this Schema<TIn, long> schema)
        where TIn : notnull =>
        Rules.Add(
            schema,
            static value => value > 0L,
            ViolationCode.OutOfRange,
            ExpectedPositive);

    /// <summary>Requires a decimal to be greater than zero.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// The overload to reach for on a price or a quantity, since the comparison is
    /// exact and a value that reads as zero is zero.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, decimal> Positive<TIn>(
        this Schema<TIn, decimal> schema) where TIn : notnull =>
        Rules.Add(
            schema,
            static value => value > 0m,
            ViolationCode.OutOfRange,
            ExpectedPositive);

    /// <summary>Requires a double-precision number to be greater than zero.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// Rejects <c>NaN</c>, which compares false against everything, and accepts
    /// positive infinity, which does not. Rejects a value small enough to have
    /// rounded to zero on its way in, which is a reason to prefer a decimal for
    /// anything a person will read as an amount.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, double> Positive<TIn>(
        this Schema<TIn, double> schema) where TIn : notnull =>
        Rules.Add(
            schema,
            static value => value > 0d,
            ViolationCode.OutOfRange,
            ExpectedPositive);

    /// <summary>Requires a 32-bit integer to be less than zero.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, int> Negative<TIn>(this Schema<TIn, int> schema)
        where TIn : notnull =>
        Rules.Add(
            schema,
            static value => value < 0,
            ViolationCode.OutOfRange,
            ExpectedNegative);

    /// <summary>Requires a 64-bit integer to be less than zero.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, long> Negative<TIn>(this Schema<TIn, long> schema)
        where TIn : notnull =>
        Rules.Add(
            schema,
            static value => value < 0L,
            ViolationCode.OutOfRange,
            ExpectedNegative);

    /// <summary>Requires a decimal to be less than zero.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, decimal> Negative<TIn>(
        this Schema<TIn, decimal> schema) where TIn : notnull =>
        Rules.Add(
            schema,
            static value => value < 0m,
            ViolationCode.OutOfRange,
            ExpectedNegative);

    /// <summary>Requires a double-precision number to be less than zero.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the rule to.</param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// Rejects <c>NaN</c> and accepts negative infinity, for the same reason the
    /// positive overload does.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, double> Negative<TIn>(
        this Schema<TIn, double> schema) where TIn : notnull =>
        Rules.Add(
            schema,
            static value => value < 0d,
            ViolationCode.OutOfRange,
            ExpectedNegative);
}
