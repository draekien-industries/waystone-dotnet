namespace Waystone.Monads.Schemas;

using System;
using System.Runtime.CompilerServices;
using Waystone.Monads.Options;

/// <summary>The entry point for building schemas and binding them to values.</summary>
/// <remarks>
/// <para>
/// Carries only static members. It is an abstract class rather than a static one
/// because the source generator nests a subclass of it inside each schema class,
/// which is how <c>Schema.Fields</c> reaches an unbounded set of arities — a
/// static class cannot be inherited, and inheritance is the whole mechanism.
/// Static members are inherited in C#, so every member here resolves through
/// that nested class unchanged.
/// </para>
/// <para>
/// Deriving from it gains nothing, since it declares no instance members.
/// </para>
/// </remarks>
public abstract partial class Schema
{
    /// <summary>Creates a schema entry point.</summary>
    /// <remarks>
    /// Protected rather than private so the generated nested class can derive from
    /// it. A private or <c>private protected</c> constructor makes the type
    /// underivable from the consumer's assembly, which is exactly where that class
    /// lives.
    /// </remarks>
    protected Schema()
    {
    }

    /// <summary>Requires every one of several schemas to accept the value.</summary>
    /// <typeparam name="TIn">The type every branch accepts.</typeparam>
    /// <typeparam name="TOut">The type every branch produces.</typeparam>
    /// <param name="branches">
    /// The schemas to apply, all against the same input. Each contributes its
    /// failures, so a value breaking three of them is reported three times.
    /// </param>
    /// <returns>A schema passing only when every branch passes.</returns>
    /// <remarks>
    /// <para>
    /// For a conjunction of independent checks, not for a chain. The branches do
    /// not feed one another; they all see the same input, and only the
    /// <b>first</b> branch's value is kept. So a branch that transforms is a
    /// mistake here — put transforms on the chain with <c>Check</c> and
    /// <c>Transform</c>, where each step sees the one before it.
    /// </para>
    /// <para>
    /// Every branch shares <typeparamref name="TIn" /> and
    /// <typeparamref name="TOut" />. C# has no union type, so a combinator cannot
    /// widen.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="branches" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// If <paramref name="branches" /> is empty or holds a null.
    /// </exception>
    public static Schema<TIn, TOut> All<TIn, TOut>(
        params Schema<TIn, TOut>[] branches)
        where TIn : notnull where TOut : notnull =>
        new AllSchema<TIn, TOut>(branches);

    /// <summary>Requires at least one of several schemas to accept the value.</summary>
    /// <typeparam name="TIn">The type every branch accepts.</typeparam>
    /// <typeparam name="TOut">The type every branch produces.</typeparam>
    /// <param name="branches">
    /// The alternatives, tried in the order given. The first to pass wins and the
    /// rest are not run, so put the cheapest or likeliest first.
    /// </param>
    /// <returns>A schema passing as soon as one branch passes.</returns>
    /// <remarks>
    /// <para>
    /// When every branch fails, the report gets one violation at this schema's own
    /// path saying no alternative matched, and each branch's own failures beneath
    /// it under a numbered segment — <c>contact[0].email</c> for the first branch.
    /// They are deliberately not flattened onto the field's own path: a
    /// three-branch union would otherwise drop a dozen irrelevant failures where a
    /// caller reading <c>ByPath()</c> expects one, which is the most complained-of
    /// part of Zod's output. Call <c>WithMessage</c> on the result to replace the
    /// lot with one sentence.
    /// </para>
    /// <para>
    /// Named <c>Any</c> rather than <c>Some</c>, which in this library already
    /// means an <c>Option</c> holding a value.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="branches" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// If <paramref name="branches" /> is empty or holds a null.
    /// </exception>
    public static Schema<TIn, TOut> Any<TIn, TOut>(
        params Schema<TIn, TOut>[] branches)
        where TIn : notnull where TOut : notnull =>
        new AnySchema<TIn, TOut>(branches);

    /// <summary>Binds a value that must be present to a schema.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <typeparam name="TOut">The type the schema produces.</typeparam>
    /// <param name="value">
    /// The value to parse. A <c>None</c> reports the field as absent without
    /// running the schema.
    /// </param>
    /// <param name="schema">The rule to apply once the value is known to be present.</param>
    /// <param name="message">
    /// Overrides the message reported when the value is absent. Supports
    /// <c>{Path}</c> and <c>{Code}</c>. Default: <c>Expected {Path} to be
    /// present.</c>
    /// </param>
    /// <param name="valueExpression">
    /// Supplied by the compiler. Leave it unset — passing it by hand overrides the
    /// derived path segment, which <c>Named</c> does more clearly.
    /// </param>
    /// <returns>
    /// A field yielding the parsed value, for a <c>Schema.Fields</c> slot.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="value" /> or <paramref name="schema" /> is null.
    /// </exception>
    public static Field<TOut> Required<TIn, TOut>(
        Option<TIn> value,
        Schema<TIn, TOut> schema,
        string? message = null,
        [CallerArgumentExpression(nameof(value))]
        string? valueExpression = null)
        where TIn : notnull where TOut : notnull =>
        new RequiredField<TIn, TOut>(
            value,
            schema,
            PathName.From(valueExpression),
            message);

    /// <summary>Binds a nullable reference that must be present to a schema.</summary>
    /// <typeparam name="TIn">
    /// The reference type the schema accepts. The constraint is what selects this
    /// overload over the value type one.
    /// </typeparam>
    /// <typeparam name="TOut">The type the schema produces.</typeparam>
    /// <param name="value">
    /// The value to parse. Null reports the field as absent without running the
    /// schema, so the schema never sees null.
    /// </param>
    /// <param name="schema">The rule to apply once the value is known to be present.</param>
    /// <param name="message">
    /// Overrides the message reported when the value is absent. Default:
    /// <c>Expected {Path} to be present.</c>
    /// </param>
    /// <param name="valueExpression">Supplied by the compiler. Leave it unset.</param>
    /// <returns>
    /// A field yielding the parsed value, for a <c>Schema.Fields</c> slot.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Field<TOut> Required<TIn, TOut>(
        TIn? value,
        Schema<TIn, TOut> schema,
        string? message = null,
        [CallerArgumentExpression(nameof(value))]
        string? valueExpression = null)
        where TIn : class where TOut : notnull =>
        Required(
            Option.FromNullable(value),
            schema,
            message,
            valueExpression);

    /// <summary>Binds a nullable value type that must be present to a schema.</summary>
    /// <typeparam name="TIn">
    /// The underlying value type the schema accepts. The parameter is its
    /// <see cref="Nullable{T}" /> form, and the constraint is what selects this
    /// overload over the reference type one.
    /// </typeparam>
    /// <typeparam name="TOut">The type the schema produces.</typeparam>
    /// <param name="value">
    /// The value to parse. No value reports the field as absent without running
    /// the schema.
    /// </param>
    /// <param name="schema">The rule to apply once the value is known to be present.</param>
    /// <param name="message">
    /// Overrides the message reported when the value is absent. Default:
    /// <c>Expected {Path} to be present.</c>
    /// </param>
    /// <param name="valueExpression">Supplied by the compiler. Leave it unset.</param>
    /// <returns>
    /// A field yielding the parsed value, for a <c>Schema.Fields</c> slot.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Field<TOut> Required<TIn, TOut>(
        TIn? value,
        Schema<TIn, TOut> schema,
        string? message = null,
        [CallerArgumentExpression(nameof(value))]
        string? valueExpression = null)
        where TIn : struct where TOut : notnull =>
        Required(
            Option.FromNullable(value),
            schema,
            message,
            valueExpression);

    /// <summary>Binds a value that may be absent to a schema.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <typeparam name="TOut">The type the schema produces.</typeparam>
    /// <param name="value">
    /// The value to parse. A <c>None</c> passes without running the schema.
    /// </param>
    /// <param name="schema">The rule to apply when the value is present.</param>
    /// <param name="valueExpression">Supplied by the compiler. Leave it unset.</param>
    /// <returns>
    /// A field yielding <c>Option&lt;TOut&gt;</c>, so absence reaches the
    /// constructed object as a <c>None</c> rather than as null.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="value" /> or <paramref name="schema" /> is null.
    /// </exception>
    public static Field<Option<TOut>> Optional<TIn, TOut>(
        Option<TIn> value,
        Schema<TIn, TOut> schema,
        [CallerArgumentExpression(nameof(value))]
        string? valueExpression = null)
        where TIn : notnull where TOut : notnull =>
        new OptionalField<TIn, TOut>(
            value,
            schema,
            PathName.From(valueExpression));

    /// <summary>Binds a nullable reference that may be absent to a schema.</summary>
    /// <typeparam name="TIn">
    /// The reference type the schema accepts. The constraint is what selects this
    /// overload over the value type one.
    /// </typeparam>
    /// <typeparam name="TOut">The type the schema produces.</typeparam>
    /// <param name="value">
    /// The value to parse. Null passes without running the schema, so the schema
    /// never sees null.
    /// </param>
    /// <param name="schema">The rule to apply when the value is present.</param>
    /// <param name="valueExpression">Supplied by the compiler. Leave it unset.</param>
    /// <returns>A field yielding <c>Option&lt;TOut&gt;</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Field<Option<TOut>> Optional<TIn, TOut>(
        TIn? value,
        Schema<TIn, TOut> schema,
        [CallerArgumentExpression(nameof(value))]
        string? valueExpression = null)
        where TIn : class where TOut : notnull =>
        Optional(Option.FromNullable(value), schema, valueExpression);

    /// <summary>Binds a nullable value type that may be absent to a schema.</summary>
    /// <typeparam name="TIn">
    /// The underlying value type the schema accepts. The parameter is its
    /// <see cref="Nullable{T}" /> form, and the constraint is what selects this
    /// overload over the reference type one.
    /// </typeparam>
    /// <typeparam name="TOut">The type the schema produces.</typeparam>
    /// <param name="value">
    /// The value to parse. No value passes without running the schema.
    /// </param>
    /// <param name="schema">The rule to apply when the value is present.</param>
    /// <param name="valueExpression">Supplied by the compiler. Leave it unset.</param>
    /// <returns>A field yielding <c>Option&lt;TOut&gt;</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Field<Option<TOut>> Optional<TIn, TOut>(
        TIn? value,
        Schema<TIn, TOut> schema,
        [CallerArgumentExpression(nameof(value))]
        string? valueExpression = null)
        where TIn : struct where TOut : notnull =>
        Optional(Option.FromNullable(value), schema, valueExpression);

    /// <summary>Requires that a value was not supplied at all.</summary>
    /// <typeparam name="T">The type of the value that must be absent.</typeparam>
    /// <param name="value">
    /// The value to check. A <c>Some</c> fails the field; the value reaches the
    /// message through <c>{Received}</c>.
    /// </param>
    /// <param name="message">
    /// Why the field is not accepted here. Required, because no derivable text
    /// explains a field that exists on the input and must not be set. Supports
    /// <c>{Path}</c>, <c>{Received}</c> and <c>{Code}</c>.
    /// </param>
    /// <param name="valueExpression">Supplied by the compiler. Leave it unset.</param>
    /// <returns>
    /// A field yielding <see cref="Checked" />, so pass it to <c>Refine</c> rather
    /// than taking a slot in the <c>Into</c> lambda.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="value" /> or <paramref name="message" /> is null.
    /// </exception>
    public static Field<Checked> Forbidden<T>(
        Option<T> value,
        string message,
        [CallerArgumentExpression(nameof(value))]
        string? valueExpression = null) where T : notnull =>
        new ForbiddenField<T>(
            value,
            PathName.From(valueExpression),
            message);

    /// <summary>Requires that a nullable reference was not supplied at all.</summary>
    /// <typeparam name="T">
    /// The reference type that must be absent. The constraint is what selects this
    /// overload over the value type one.
    /// </typeparam>
    /// <param name="value">The value to check. Anything but null fails the field.</param>
    /// <param name="message">Why the field is not accepted here.</param>
    /// <param name="valueExpression">Supplied by the compiler. Leave it unset.</param>
    /// <returns>A field yielding <see cref="Checked" />.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="message" /> is null.
    /// </exception>
    public static Field<Checked> Forbidden<T>(
        T? value,
        string message,
        [CallerArgumentExpression(nameof(value))]
        string? valueExpression = null) where T : class =>
        Forbidden(Option.FromNullable(value), message, valueExpression);

    /// <summary>Requires that a nullable value type was not supplied at all.</summary>
    /// <typeparam name="T">
    /// The underlying value type that must be absent. The parameter is its
    /// <see cref="Nullable{T}" /> form, and the constraint is what selects this
    /// overload over the reference type one.
    /// </typeparam>
    /// <param name="value">The value to check. Having a value fails the field.</param>
    /// <param name="message">Why the field is not accepted here.</param>
    /// <param name="valueExpression">Supplied by the compiler. Leave it unset.</param>
    /// <returns>A field yielding <see cref="Checked" />.</returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="message" /> is null.
    /// </exception>
    public static Field<Checked> Forbidden<T>(
        T? value,
        string message,
        [CallerArgumentExpression(nameof(value))]
        string? valueExpression = null) where T : struct =>
        Forbidden(Option.FromNullable(value), message, valueExpression);

    /// <summary>Applies a shared set of rules to the whole subject.</summary>
    /// <typeparam name="T">The subject's type.</typeparam>
    /// <param name="subject">The value the rules run against.</param>
    /// <param name="rules">
    /// A schema over the subject itself. Its violations are reported at the
    /// subject's own path rather than under a field name, which is what makes it
    /// the right home for a cross-field rule.
    /// </param>
    /// <returns>
    /// A field yielding <see cref="Checked" />, so pass it to <c>Refine</c>. The
    /// value the rules produce is discarded — only their violations matter.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="rules" /> is null.
    /// </exception>
    public static Field<Checked> Extend<T>(T subject, Schema<T, T> rules)
        where T : notnull =>
        new ExtendField<T>(subject, rules);
}
