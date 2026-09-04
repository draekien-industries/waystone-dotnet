namespace Waystone.Monads.Schemas;

using System;

public abstract partial class Schema
{
    /// <summary>Gets a schema accepting any string, as a base for text rules.</summary>
    /// <value>
    /// A schema that produces its input unchanged. Chain <c>Trim</c>,
    /// <c>NotEmpty</c>, <c>MinLength</c>, <c>MaxLength</c> or <c>Matches</c> onto
    /// it.
    /// </value>
    /// <remarks>
    /// Checks nothing on its own, and there is nothing for it to check: the type
    /// system has already established that the value is a string, and a null never
    /// reaches a schema because <c>Required</c> and <c>Optional</c> both stop it.
    /// Reaching for this and adding no rule is therefore the same as adding no
    /// schema, which is occasionally what you want for a free-text field.
    /// </remarks>
    public static Schema<string, string> Text { get; } = For<string>();

    /// <summary>Gets a schema accepting either boolean value.</summary>
    /// <value>A schema that produces its input unchanged.</value>
    /// <remarks>
    /// Exists so a boolean field reads like its neighbours in a
    /// <c>Schema.Fields</c> list rather than as a bare <c>For&lt;bool&gt;()</c>. To
    /// insist on one of the two values, chain
    /// <c>Check(static accepted =&gt; accepted, ...)</c>, which is how a
    /// terms-of-service checkbox is written.
    /// </remarks>
    public static Schema<bool, bool> Bool { get; } = For<bool>();

    /// <summary>Gets a schema accepting any UUID, as a base for identifier rules.</summary>
    /// <value>
    /// A schema that produces its input unchanged. Chain <c>NotEmpty</c> to reject
    /// <see cref="Guid.Empty" />, and <c>IsVersion4</c> to insist on how it was
    /// generated.
    /// </value>
    /// <remarks>
    /// <para>
    /// Accepts <see cref="Guid.Empty" /> unless you say otherwise. An unset
    /// <see cref="Guid" /> field deserialises to <see cref="Guid.Empty" /> rather
    /// than to null, so a required identifier that omits <c>NotEmpty</c> accepts a
    /// value the sender never supplied.
    /// </para>
    /// <para>
    /// Named for the standard rather than for the role, because the rules on it are
    /// about the UUID layout — which version generated it, and whether it is set. A
    /// schema for an identifier that happens not to be a UUID starts at
    /// <c>For&lt;T&gt;()</c> instead.
    /// </para>
    /// </remarks>
    public static Schema<Guid, Guid> Uuid { get; } = For<Guid>();

    /// <summary>Gets a schema accepting any instant, as a base for temporal rules.</summary>
    /// <value>
    /// A schema that produces its input unchanged. Chain <c>Before</c> or
    /// <c>After</c> to bound it.
    /// </value>
    /// <remarks>
    /// A <see cref="DateTimeOffset" /> rather than a <see cref="DateTime" />,
    /// because the offset is what makes two instants comparable across senders.
    /// Comparison follows <see cref="DateTimeOffset" />'s own ordering, which is by
    /// the instant in time rather than by the wall clock, so midday in Sydney sorts
    /// before midday in London.
    /// </remarks>
    public static Schema<DateTimeOffset, DateTimeOffset> Timestamp { get; } =
        For<DateTimeOffset>();

    /// <summary>Creates a schema accepting any value of a type, as a base for your own rules.</summary>
    /// <typeparam name="T">The type to accept and produce.</typeparam>
    /// <returns>A schema that produces its input unchanged.</returns>
    /// <remarks>
    /// The starting point for a type with no built-in of its own — a domain type, a
    /// framework type, an interface. Chain <c>Check</c> and <c>Transform</c> onto it.
    /// The result is cached per <typeparamref name="T" />, so calling this in a loop
    /// allocates nothing.
    /// </remarks>
    public static Schema<T, T> For<T>() where T : notnull =>
        IdentitySchema<T>.Instance;

    /// <summary>Creates a schema accepting only the values an enumeration recognises.</summary>
    /// <typeparam name="T">The enumeration to accept and produce.</typeparam>
    /// <returns>
    /// A schema reporting <c>schema_violation.mismatched</c> for a value the
    /// enumeration does not recognise.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Worth reaching for because the type system does not do this job. An
    /// enumeration is an integer underneath, so a deserialiser will hand you
    /// <c>(Status)97</c> from a request body without complaint, and every
    /// <c>switch</c> over it afterwards falls to its default arm.
    /// </para>
    /// <para>
    /// What counts as recognised depends on <c>[Flags]</c>. Without it, the value has
    /// to equal a declared member. With it, any combination of the bits the members
    /// declare is accepted — <c>Read | Write</c> passes without being declared in its
    /// own right — while a bit no member declares still fails.
    /// </para>
    /// <para>
    /// Zero is the case to watch on a flags enumeration. It passes only where a
    /// member declares it, because zero is what a deserialiser produces for a field
    /// the payload left out. Declare a <c>None = 0</c> member if an empty
    /// combination is legitimate.
    /// </para>
    /// </remarks>
    public static Schema<T, T> Enum<T>() where T : struct, System.Enum =>
        DeclaredMembers<T>.Instance;

    /// <summary>Groups the schemas for the built-in numeric types.</summary>
    /// <remarks>
    /// Nested so the four spellings sit together and none of them has to be called
    /// <c>Schema.Int32</c>, which reads as a conversion rather than as a schema.
    /// All four share the comparison rules — <c>AtLeast</c>, <c>AtMost</c>,
    /// <c>GreaterThan</c>, <c>LessThan</c> — along with <c>Positive</c> and
    /// <c>Negative</c>.
    /// </remarks>
    public static class Number
    {
        /// <summary>Gets a schema accepting any 32-bit signed integer.</summary>
        /// <value>A schema that produces its input unchanged.</value>
        /// <remarks>
        /// The default choice for a count, an age or a quantity. Reach for
        /// <see cref="Int64" /> when the value is an identifier or a total that can
        /// outgrow roughly two billion.
        /// </remarks>
        public static Schema<int, int> Int32 { get; } = For<int>();

        /// <summary>Gets a schema accepting any 64-bit signed integer.</summary>
        /// <value>A schema that produces its input unchanged.</value>
        public static Schema<long, long> Int64 { get; } = For<long>();

        /// <summary>Gets a schema accepting any decimal number.</summary>
        /// <value>A schema that produces its input unchanged.</value>
        /// <remarks>
        /// The right type for money. Comparison is exact, so a bound of
        /// <c>0.1m</c> means what it says — unlike <see cref="Double" />, where it
        /// does not.
        /// </remarks>
        public static Schema<decimal, decimal> Decimal { get; } =
            For<decimal>();

        /// <summary>Gets a schema accepting any double-precision number.</summary>
        /// <value>A schema that produces its input unchanged.</value>
        /// <remarks>
        /// Accepts <c>NaN</c> and both infinities, and every
        /// comparison rule returns false against <c>NaN</c> — so a <c>NaN</c> fails
        /// <c>AtLeast</c> and <c>AtMost</c> alike, and passes neither by accident.
        /// Rejecting it outright takes a <c>Check</c>. Prefer
        /// <see cref="Decimal" /> for any value a person will read as an amount.
        /// </remarks>
        public static Schema<double, double> Double { get; } = For<double>();
    }
}
