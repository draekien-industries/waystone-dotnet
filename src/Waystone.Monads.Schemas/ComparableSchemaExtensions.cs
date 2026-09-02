namespace Waystone.Monads.Schemas;

using System;

/// <summary>Bounds for a schema producing a value that can be ordered.</summary>
/// <remarks>
/// <para>
/// One family covering every ordered type rather than a set per type. The
/// constraint is <see cref="IComparable{T}" />, so these reach all four numerics,
/// both temporals, <see cref="string" />, <see cref="TimeSpan" /> and any domain
/// type that implements it — including one reached through <c>Transform</c>, which
/// is where they earn their keep. A per-type set would be the same four rules
/// copied six ways and would still miss the domain type.
/// </para>
/// <para>
/// Ordering is whatever <see cref="IComparable{T}.CompareTo" /> says it is. For
/// <see cref="string" /> that is the current culture's, which sorts differently
/// from <see cref="StringComparer.Ordinal" /> and differently again on another
/// machine. Bound text with <c>MinLength</c> and <c>MaxLength</c>, and keep these
/// for values that are genuinely numeric or temporal.
/// </para>
/// <para>
/// All of them report <c>schema_violation.out-of-range</c> and supply the bound to
/// <c>{Expected}</c>. Use <c>Between</c> for a range rather than
/// <c>AtLeast(1).AtMost(10)</c>: the two spellings accept the same values, but the
/// chain is two refinements and so reports the range as two separate failures,
/// while <c>Between</c> reports the one a caller can act on.
/// </para>
/// </remarks>
public static class ComparableSchemaExtensions
{
    /// <summary>Requires a value to be no smaller than a bound.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <typeparam name="T">The ordered type the schema produces.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The smallest accepted value. Inclusive — a value equal to it passes.
    /// Reaches the message through <c>{Expected}</c>.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <remarks>
    /// Default message: <c>Expected {Path} to be at least {Expected}, but got
    /// {Received}.</c> Replace it with <c>WithMessage</c>, which renders
    /// <c>{Expected}</c> literally, so restate the bound in the text.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, T> AtLeast<TIn, T>(
        this Schema<TIn, T> schema,
        T bound)
        where TIn : notnull where T : notnull, IComparable<T> =>
        Rules.Add(
            schema,
            value => value.CompareTo(bound) >= 0,
            ViolationCode.OutOfRange,
            "Expected {Path} to be at least {Expected}, but got {Received}.",
            bound);

    /// <summary>Requires a value to be no larger than a bound.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <typeparam name="T">The ordered type the schema produces.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The largest accepted value. Inclusive — a value equal to it passes.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <remarks>
    /// Default message: <c>Expected {Path} to be at most {Expected}, but got
    /// {Received}.</c>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, T> AtMost<TIn, T>(
        this Schema<TIn, T> schema,
        T bound)
        where TIn : notnull where T : notnull, IComparable<T> =>
        Rules.Add(
            schema,
            value => value.CompareTo(bound) <= 0,
            ViolationCode.OutOfRange,
            "Expected {Path} to be at most {Expected}, but got {Received}.",
            bound);

    /// <summary>Requires a value to exceed a bound.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <typeparam name="T">The ordered type the schema produces.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The value to exceed. Exclusive — a value equal to it fails. Use
    /// <see cref="AtLeast{TIn,T}" /> where the bound itself is acceptable, which on
    /// an integer is the same rule written without an off-by-one.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, T> GreaterThan<TIn, T>(
        this Schema<TIn, T> schema,
        T bound)
        where TIn : notnull where T : notnull, IComparable<T> =>
        Rules.Add(
            schema,
            value => value.CompareTo(bound) > 0,
            ViolationCode.OutOfRange,
            "Expected {Path} to be greater than {Expected}, but got {Received}.",
            bound);

    /// <summary>Requires a value to fall short of a bound.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <typeparam name="T">The ordered type the schema produces.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The value to fall short of. Exclusive — a value equal to it fails.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, T> LessThan<TIn, T>(
        this Schema<TIn, T> schema,
        T bound)
        where TIn : notnull where T : notnull, IComparable<T> =>
        Rules.Add(
            schema,
            value => value.CompareTo(bound) < 0,
            ViolationCode.OutOfRange,
            "Expected {Path} to be less than {Expected}, but got {Received}.",
            bound);

    /// <summary>Requires a value to fall within a range.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <typeparam name="T">The ordered type the schema produces.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="min">The smallest accepted value. Inclusive.</param>
    /// <param name="max">The largest accepted value. Inclusive.</param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <remarks>
    /// <para>
    /// Both ends are inclusive, matching <c>AtLeast</c> and <c>AtMost</c>. For a
    /// half-open range chain those two, or <c>GreaterThan</c> and <c>LessThan</c>
    /// for an open one — this rule is the common case rather than every case.
    /// </para>
    /// <para>
    /// Default message: <c>Expected {Path} to be between {Expected}, but got
    /// {Received}.</c>, where <c>{Expected}</c> renders both ends.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// If <paramref name="max" /> orders before <paramref name="min" />, which
    /// describes a range no value can satisfy. Thrown when the schema is built
    /// rather than when it parses, so the mistake surfaces at startup.
    /// </exception>
    public static Schema<TIn, T> Between<TIn, T>(
        this Schema<TIn, T> schema,
        T min,
        T max)
        where TIn : notnull where T : notnull, IComparable<T>
    {
        Rules.RequireOrdered(min, max, nameof(max));

        return Rules.Add(
            schema,
            value => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0,
            ViolationCode.OutOfRange,
            "Expected {Path} to be between {Expected}, but got {Received}.",
            $"{min} and {max}");
    }
}
