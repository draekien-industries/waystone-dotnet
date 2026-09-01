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
/// All four report <c>schema_violation.out-of-range</c> and supply the bound to
/// <c>{Expected}</c>. Both bounds together take two calls —
/// <c>AtLeast(1).AtMost(10)</c> — and a value outside both is reported twice,
/// because each is a refinement and refinements do not stop one another.
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
}
