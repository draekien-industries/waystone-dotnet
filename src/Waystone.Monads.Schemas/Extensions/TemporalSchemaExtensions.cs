namespace Waystone.Monads.Schemas;

using System;

/// <summary>Bounds for a schema producing an instant or a date.</summary>
/// <remarks>
/// <para>
/// <c>LessThan</c> and <c>GreaterThan</c> already do this work, and these say the
/// same thing in the words a reader uses about time. <c>Before</c> is
/// <c>LessThan</c> and <c>After</c> is <c>GreaterThan</c>, both exclusive;
/// <c>OnOrBefore</c> and <c>OnOrAfter</c> are the inclusive pair. All four report
/// <c>schema_violation.out-of-range</c>.
/// </para>
/// <para>
/// Reach for the inclusive pair by default when the bound is a date. A deadline of
/// the thirty-first is open on the thirty-first, and writing that as
/// <c>Before(first of next month)</c> is a calculation a reader has to redo.
/// </para>
/// <para>
/// A bound taken from the clock is captured when the schema is built. A schema
/// declared as a static field and given <c>DateTimeOffset.UtcNow</c> holds the
/// moment the process started for as long as it runs, which is a bug that only
/// shows up in production. Take the clock inside <c>Configure</c>, where the
/// schema is built per parse, or check the instant with <c>Check</c> against a
/// clock you can substitute.
/// </para>
/// </remarks>
public static class TemporalSchemaExtensions
{
    private const string ExpectedBefore =
        "Expected {Path} to be before {Expected}, but got {Received}.";

    private const string ExpectedAfter =
        "Expected {Path} to be after {Expected}, but got {Received}.";

    private const string ExpectedOnOrBefore =
        "Expected {Path} to be no later than {Expected}, but got {Received}.";

    private const string ExpectedOnOrAfter =
        "Expected {Path} to be no earlier than {Expected}, but got {Received}.";

    /// <summary>Requires an instant to fall before another.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The instant to fall before. Exclusive — an instant equal to it fails.
    /// Compared by the moment in time rather than by the wall clock, so the two
    /// values may carry different offsets and still order correctly.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, DateTimeOffset> Before<TIn>(
        this Schema<TIn, DateTimeOffset> schema,
        DateTimeOffset bound) where TIn : notnull =>
        Rules.Add(
            schema,
            value => value < bound,
            ViolationCode.OutOfRange,
            ExpectedBefore,
            bound);

    /// <summary>Requires an instant to fall after another.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The instant to fall after. Exclusive — an instant equal to it fails.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, DateTimeOffset> After<TIn>(
        this Schema<TIn, DateTimeOffset> schema,
        DateTimeOffset bound) where TIn : notnull =>
        Rules.Add(
            schema,
            value => value > bound,
            ViolationCode.OutOfRange,
            ExpectedAfter,
            bound);

    /// <summary>Requires an instant to fall no later than another.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The latest instant accepted. Inclusive — an instant equal to it passes,
    /// which is the difference from <see cref="Before{TIn}(Schema{TIn, DateTimeOffset}, DateTimeOffset)" />.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, DateTimeOffset> OnOrBefore<TIn>(
        this Schema<TIn, DateTimeOffset> schema,
        DateTimeOffset bound) where TIn : notnull =>
        Rules.Add(
            schema,
            value => value <= bound,
            ViolationCode.OutOfRange,
            ExpectedOnOrBefore,
            bound);

    /// <summary>Requires an instant to fall no earlier than another.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The earliest instant accepted. Inclusive — an instant equal to it passes,
    /// which is the difference from <see cref="After{TIn}(Schema{TIn, DateTimeOffset}, DateTimeOffset)" />.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, DateTimeOffset> OnOrAfter<TIn>(
        this Schema<TIn, DateTimeOffset> schema,
        DateTimeOffset bound) where TIn : notnull =>
        Rules.Add(
            schema,
            value => value >= bound,
            ViolationCode.OutOfRange,
            ExpectedOnOrAfter,
            bound);

#if NET8_0_OR_GREATER
    /// <summary>Requires a date to fall before another.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The date to fall before. Exclusive — the same date fails. Use
    /// <c>OnOrBefore</c> to accept it.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <remarks>
    /// Not available on netstandard2.0, for the same reason
    /// <see cref="Schema.Date" /> is not.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, DateOnly> Before<TIn>(
        this Schema<TIn, DateOnly> schema,
        DateOnly bound) where TIn : notnull =>
        Rules.Add(
            schema,
            value => value < bound,
            ViolationCode.OutOfRange,
            ExpectedBefore,
            bound);

    /// <summary>Requires a date to fall after another.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The date to fall after. Exclusive — the same date fails. Use
    /// <c>OnOrAfter</c> to accept it.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <remarks>
    /// Not available on netstandard2.0, for the same reason
    /// <see cref="Schema.Date" /> is not.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, DateOnly> After<TIn>(
        this Schema<TIn, DateOnly> schema,
        DateOnly bound) where TIn : notnull =>
        Rules.Add(
            schema,
            value => value > bound,
            ViolationCode.OutOfRange,
            ExpectedAfter,
            bound);

    /// <summary>Requires a date to fall on or before another.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The latest date accepted. Inclusive, so this is the rule a deadline wants:
    /// the closing date itself is still open.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <remarks>
    /// Not available on netstandard2.0, for the same reason
    /// <see cref="Schema.Date" /> is not.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, DateOnly> OnOrBefore<TIn>(
        this Schema<TIn, DateOnly> schema,
        DateOnly bound) where TIn : notnull =>
        Rules.Add(
            schema,
            value => value <= bound,
            ViolationCode.OutOfRange,
            ExpectedOnOrBefore,
            bound);

    /// <summary>Requires a date to fall on or after another.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema to add the bound to.</param>
    /// <param name="bound">
    /// The earliest date accepted. Inclusive, so an effective date passes on the
    /// day it takes effect.
    /// </param>
    /// <returns>A schema that applies this bound after everything already on it.</returns>
    /// <remarks>
    /// Not available on netstandard2.0, for the same reason
    /// <see cref="Schema.Date" /> is not.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, DateOnly> OnOrAfter<TIn>(
        this Schema<TIn, DateOnly> schema,
        DateOnly bound) where TIn : notnull =>
        Rules.Add(
            schema,
            value => value >= bound,
            ViolationCode.OutOfRange,
            ExpectedOnOrAfter,
            bound);
#endif
}
