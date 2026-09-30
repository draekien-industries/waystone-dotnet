namespace Waystone.Monads.Schemas;

using System;
using System.Globalization;

public static partial class TextSchemaExtensions
{
    /// <summary>Converts the text to a <see cref="Guid" />.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <returns>A schema producing the <see cref="Guid" /> the text spells.</returns>
    /// <remarks>
    /// <para>
    /// Accepts all five layouts <see cref="Guid.ToString(string)" /> writes, in
    /// either case: <c>0f8fad5b-d9cb-469f-a165-70867728950e</c>, the same digits
    /// without hyphens, wrapped in braces or parentheses, and the hexadecimal
    /// structure form. Whitespace around the value is ignored.
    /// </para>
    /// <para>
    /// A conversion, so text it cannot read leaves no value and nothing after it
    /// runs. Follow it with <see cref="Schema{TIn,TOut}.Pipe{TNext}" /> to apply a
    /// <see cref="Guid" /> schema you have already declared, such as one requiring
    /// <c>NotEmpty</c>. Reports <c>schema_violation.malformed</c> with the default
    /// message <c>Expected {Path} to be a UUID, but got {Received}.</c>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, Guid> ToUuid<TIn>(this Schema<TIn, string> schema)
        where TIn : notnull =>
        Parsed<TIn, Guid>(
            schema,
            Guid.TryParse,
            "Expected {Path} to be a UUID, but got {Received}.");

    /// <summary>Converts the text <c>"true"</c> or <c>"false"</c> to a <see cref="bool" />.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <returns>A schema producing the <see cref="bool" /> the text spells.</returns>
    /// <remarks>
    /// <para>
    /// Matches <c>"true"</c> and <c>"false"</c> in any case, ignoring whitespace
    /// around the value. <c>"1"</c>, <c>"0"</c>, <c>"yes"</c> and <c>"on"</c> are
    /// rejected, so a form checkbox posting <c>"on"</c> needs <c>Transform</c>
    /// instead.
    /// </para>
    /// <para>
    /// A conversion, so text it cannot read leaves no value and nothing after it
    /// runs. Reports <c>schema_violation.malformed</c> with the default message
    /// <c>Expected {Path} to be true or false, but got {Received}.</c>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, bool> ToBool<TIn>(this Schema<TIn, string> schema)
        where TIn : notnull =>
        Parsed<TIn, bool>(
            schema,
            bool.TryParse,
            "Expected {Path} to be true or false, but got {Received}.");

    /// <summary>Converts the text to an <see cref="int" />, reading it in the invariant culture.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <returns>A schema producing the <see cref="int" /> the text spells.</returns>
    /// <remarks>
    /// The same result on every server, whatever its culture, which is what text
    /// from JSON, a query string or a header needs. Parse text a person typed with
    /// <see cref="ToInt32{TIn}(Schema{TIn,string},IFormatProvider)" /> instead. See
    /// that overload for what is accepted and reported.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, int> ToInt32<TIn>(this Schema<TIn, string> schema)
        where TIn : notnull =>
        schema.ToInt32(CultureInfo.InvariantCulture);

    /// <summary>Converts the text to an <see cref="int" />, reading it in the culture you pass.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <param name="provider">
    /// The culture whose negative sign the text uses, such as
    /// <see cref="CultureInfo.CurrentCulture" /> for text a person typed.
    /// </param>
    /// <returns>A schema producing the <see cref="int" /> the text spells.</returns>
    /// <remarks>
    /// <para>
    /// Reads the text with <see cref="NumberStyles.Integer" />, as
    /// <see cref="int.TryParse(string, out int)" /> does: digits, a leading sign and
    /// whitespace around the value. A decimal point, a thousands separator and
    /// exponent notation are rejected, so <c>"1.0"</c> and <c>"1,000"</c> fail.
    /// </para>
    /// <para>
    /// A conversion, so text it cannot read leaves no value and nothing after it
    /// runs. A number outside the range of <see cref="int" /> is rejected rather than
    /// wrapped. Reports <c>schema_violation.malformed</c> with the default message
    /// <c>Expected {Path} to be a whole number from -2147483648 to 2147483647, but
    /// got {Received}.</c>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="provider" /> is null.
    /// </exception>
    public static Schema<TIn, int> ToInt32<TIn>(
        this Schema<TIn, string> schema,
        IFormatProvider provider) where TIn : notnull
    {
        if (provider is null) throw new ArgumentNullException(nameof(provider));

        return Parsed(
            schema,
            (string text, out int value) =>
                int.TryParse(text, NumberStyles.Integer, provider, out value),
            "Expected {Path} to be a whole number from -2147483648 to 2147483647, but got {Received}.");
    }

    /// <summary>Converts the text to a <see cref="long" />, reading it in the invariant culture.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <returns>A schema producing the <see cref="long" /> the text spells.</returns>
    /// <remarks>
    /// The same result on every server, whatever its culture. Parse text a person
    /// typed with <see cref="ToInt64{TIn}(Schema{TIn,string},IFormatProvider)" />
    /// instead. See that overload for what is accepted and reported.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, long> ToInt64<TIn>(this Schema<TIn, string> schema)
        where TIn : notnull =>
        schema.ToInt64(CultureInfo.InvariantCulture);

    /// <summary>Converts the text to a <see cref="long" />, reading it in the culture you pass.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <param name="provider">
    /// The culture whose negative sign the text uses, such as
    /// <see cref="CultureInfo.CurrentCulture" /> for text a person typed.
    /// </param>
    /// <returns>A schema producing the <see cref="long" /> the text spells.</returns>
    /// <remarks>
    /// <para>
    /// Reads the text with <see cref="NumberStyles.Integer" />: digits, a leading
    /// sign and whitespace around the value. A decimal point, a thousands separator
    /// and exponent notation are rejected.
    /// </para>
    /// <para>
    /// A conversion, so text it cannot read leaves no value and nothing after it
    /// runs. A number outside the range of <see cref="long" /> is rejected. Reports
    /// <c>schema_violation.malformed</c> with the default message <c>Expected {Path}
    /// to be a whole number from -9223372036854775808 to 9223372036854775807, but got
    /// {Received}.</c>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="provider" /> is null.
    /// </exception>
    public static Schema<TIn, long> ToInt64<TIn>(
        this Schema<TIn, string> schema,
        IFormatProvider provider) where TIn : notnull
    {
        if (provider is null) throw new ArgumentNullException(nameof(provider));

        return Parsed(
            schema,
            (string text, out long value) =>
                long.TryParse(text, NumberStyles.Integer, provider, out value),
            "Expected {Path} to be a whole number from -9223372036854775808 to 9223372036854775807, but got {Received}.");
    }

    /// <summary>Converts the text to a <see cref="decimal" />, reading it in the invariant culture.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <returns>A schema producing the <see cref="decimal" /> the text spells.</returns>
    /// <remarks>
    /// The same result on every server: <c>"1.5"</c> is one and a half and
    /// <c>"1,5"</c> is fifteen, because the invariant culture's decimal point is a
    /// full stop and its thousands separator a comma. Parse text a person typed with
    /// <see cref="ToDecimal{TIn}(Schema{TIn,string},IFormatProvider)" /> instead.
    /// See that overload for what is accepted and reported.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, decimal> ToDecimal<TIn>(
        this Schema<TIn, string> schema) where TIn : notnull =>
        schema.ToDecimal(CultureInfo.InvariantCulture);

    /// <summary>Converts the text to a <see cref="decimal" />, reading it in the culture you pass.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <param name="provider">
    /// The culture whose decimal point, thousands separator and negative sign the
    /// text uses. With <c>de-DE</c>, <c>"1,5"</c> is one and a half.
    /// </param>
    /// <returns>A schema producing the <see cref="decimal" /> the text spells.</returns>
    /// <remarks>
    /// <para>
    /// Reads the text with <see cref="NumberStyles.Number" />, as
    /// <see cref="decimal.TryParse(string, out decimal)" /> does: a sign, thousands
    /// separators, a decimal point and whitespace around the value. A currency
    /// symbol and exponent notation are rejected, so <c>"$5"</c> and <c>"1e3"</c>
    /// fail. More than 29 significant digits are rounded to the nearest value.
    /// </para>
    /// <para>
    /// A conversion, so text it cannot read leaves no value and nothing after it
    /// runs. Reports <c>schema_violation.malformed</c> with the default message
    /// <c>Expected {Path} to be a decimal number, but got {Received}.</c>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="provider" /> is null.
    /// </exception>
    public static Schema<TIn, decimal> ToDecimal<TIn>(
        this Schema<TIn, string> schema,
        IFormatProvider provider) where TIn : notnull
    {
        if (provider is null) throw new ArgumentNullException(nameof(provider));

        return Parsed(
            schema,
            (string text, out decimal value) =>
                decimal.TryParse(text, NumberStyles.Number, provider, out value),
            "Expected {Path} to be a decimal number, but got {Received}.");
    }

    /// <summary>Converts the text to a <see cref="double" />, reading it in the invariant culture.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <returns>A schema producing the <see cref="double" /> the text spells.</returns>
    /// <remarks>
    /// The same result on every server, whatever its culture. Parse text a person
    /// typed with <see cref="ToDouble{TIn}(Schema{TIn,string},IFormatProvider)" />
    /// instead. See that overload for what is accepted and reported.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, double> ToDouble<TIn>(
        this Schema<TIn, string> schema) where TIn : notnull =>
        schema.ToDouble(CultureInfo.InvariantCulture);

    /// <summary>Converts the text to a <see cref="double" />, reading it in the culture you pass.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <param name="provider">
    /// The culture whose decimal point, thousands separator, negative sign and
    /// names for infinity and not-a-number the text uses.
    /// </param>
    /// <returns>A schema producing the <see cref="double" /> the text spells.</returns>
    /// <remarks>
    /// <para>
    /// Reads the text with <see cref="NumberStyles.Float" /> and
    /// <see cref="NumberStyles.AllowThousands" />, as
    /// <see cref="double.TryParse(string, out double)" /> does: a sign, thousands
    /// separators, a decimal point, exponent notation such as <c>"1e3"</c>, and
    /// whitespace around the value. A currency symbol is rejected.
    /// </para>
    /// <para>
    /// <b>The culture's names for infinity and not-a-number are accepted</b>, so in
    /// the invariant culture <c>"NaN"</c> and <c>"Infinity"</c> both pass. Follow
    /// this with <c>Check(value => !double.IsNaN(value) &amp;&amp; !double.IsInfinity(value), …)</c>,
    /// or use <see cref="ToDecimal{TIn}(Schema{TIn,string})" />, where only a finite
    /// number makes sense.
    /// </para>
    /// <para>
    /// A conversion, so text it cannot read leaves no value and nothing after it
    /// runs. Reports <c>schema_violation.malformed</c> with the default message
    /// <c>Expected {Path} to be a number, but got {Received}.</c>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="provider" /> is null.
    /// </exception>
    public static Schema<TIn, double> ToDouble<TIn>(
        this Schema<TIn, string> schema,
        IFormatProvider provider) where TIn : notnull
    {
        if (provider is null) throw new ArgumentNullException(nameof(provider));

        return Parsed(
            schema,
            (string text, out double value) =>
                double.TryParse(
                    text,
                    NumberStyles.Float | NumberStyles.AllowThousands,
                    provider,
                    out value),
            "Expected {Path} to be a number, but got {Received}.");
    }

    /// <summary>Converts the text to a <see cref="DateTimeOffset" />, reading it in the invariant culture.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <returns>A schema producing the <see cref="DateTimeOffset" /> the text spells.</returns>
    /// <remarks>
    /// Reads month and day names and the order of a numeric date the same way on
    /// every server. ISO 8601 text such as <c>"2026-10-01T09:00:00+10:00"</c> reads
    /// the same in any culture. Parse text a person typed with
    /// <see cref="ToTimestamp{TIn}(Schema{TIn,string},IFormatProvider)" /> instead.
    /// See that overload for what is accepted and reported.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, DateTimeOffset> ToTimestamp<TIn>(
        this Schema<TIn, string> schema) where TIn : notnull =>
        schema.ToTimestamp(CultureInfo.InvariantCulture);

    /// <summary>Converts the text to a <see cref="DateTimeOffset" />, reading it in the culture you pass.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <param name="provider">
    /// The culture whose date order, separators and month names the text uses.
    /// With <c>en-AU</c>, <c>"01/10/2026"</c> is the first of October.
    /// </param>
    /// <returns>A schema producing the <see cref="DateTimeOffset" /> the text spells.</returns>
    /// <remarks>
    /// <para>
    /// Parses as <see cref="DateTimeOffset.TryParse(string, IFormatProvider, DateTimeStyles, out DateTimeOffset)" />
    /// does with <see cref="DateTimeStyles.None" />. <b>Text with no offset takes the
    /// offset of the server's local time zone</b>, so <c>"2026-10-01T09:00:00"</c>
    /// names a different instant on a server in Sydney than on one in London.
    /// Where the sender must state the offset, check the text before converting it,
    /// for example with <c>Matches</c>. Once converted, the value no longer records
    /// where its offset came from.
    /// </para>
    /// <para>
    /// Text with no date takes today's date, and text with no time takes midnight.
    /// A conversion, so text it cannot read leaves no value and nothing after it
    /// runs. Reports <c>schema_violation.malformed</c> with the default message
    /// <c>Expected {Path} to be a date and time, but got {Received}.</c>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="provider" /> is null.
    /// </exception>
    public static Schema<TIn, DateTimeOffset> ToTimestamp<TIn>(
        this Schema<TIn, string> schema,
        IFormatProvider provider) where TIn : notnull
    {
        if (provider is null) throw new ArgumentNullException(nameof(provider));

        return Parsed(
            schema,
            (string text, out DateTimeOffset value) =>
                DateTimeOffset.TryParse(
                    text,
                    provider,
                    DateTimeStyles.None,
                    out value),
            "Expected {Path} to be a date and time, but got {Received}.");
    }

#if NET8_0_OR_GREATER
    /// <summary>Converts the text to a <see cref="DateOnly" />, reading it in the invariant culture.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <returns>A schema producing the <see cref="DateOnly" /> the text spells.</returns>
    /// <remarks>
    /// Only on .NET 8 and later, as <c>Schema.Date</c> is. The invariant culture
    /// reads a numeric date month first, so <c>"01/10/2026"</c> is the tenth of
    /// January; <c>"2026-10-01"</c> reads the same in any culture. Parse text a
    /// person typed with <see cref="ToDate{TIn}(Schema{TIn,string},IFormatProvider)" />
    /// instead. See that overload for what is accepted and reported.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> is null.
    /// </exception>
    public static Schema<TIn, DateOnly> ToDate<TIn>(this Schema<TIn, string> schema)
        where TIn : notnull =>
        schema.ToDate(CultureInfo.InvariantCulture);

    /// <summary>Converts the text to a <see cref="DateOnly" />, reading it in the culture you pass.</summary>
    /// <typeparam name="TIn">The type the schema accepts.</typeparam>
    /// <param name="schema">The schema whose text to convert.</param>
    /// <param name="provider">
    /// The culture whose date order, separators and month names the text uses.
    /// With <c>en-AU</c>, <c>"01/10/2026"</c> is the first of October.
    /// </param>
    /// <returns>A schema producing the <see cref="DateOnly" /> the text spells.</returns>
    /// <remarks>
    /// <para>
    /// Only on .NET 8 and later. Parses as
    /// <see cref="DateOnly.TryParse(string, IFormatProvider, DateTimeStyles, out DateOnly)" />
    /// does with <see cref="DateTimeStyles.None" />. That accepts
    /// <c>"2026-10-01T09:00:00"</c> and discards the time, so use
    /// <see cref="ToTimestamp{TIn}(Schema{TIn,string},IFormatProvider)" /> for a value
    /// whose time of day matters.
    /// </para>
    /// <para>
    /// A conversion, so text it cannot read leaves no value and nothing after it
    /// runs. Reports <c>schema_violation.malformed</c> with the default message
    /// <c>Expected {Path} to be a date, but got {Received}.</c>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="schema" /> or <paramref name="provider" /> is null.
    /// </exception>
    public static Schema<TIn, DateOnly> ToDate<TIn>(
        this Schema<TIn, string> schema,
        IFormatProvider provider) where TIn : notnull
    {
        if (provider is null) throw new ArgumentNullException(nameof(provider));

        return Parsed(
            schema,
            (string text, out DateOnly value) =>
                DateOnly.TryParse(text, provider, DateTimeStyles.None, out value),
            "Expected {Path} to be a date, but got {Received}.");
    }
#endif

    private static Schema<TIn, TNext> Parsed<TIn, TNext>(
        Schema<TIn, string> schema,
        TextParser<TNext> parse,
        string message)
        where TIn : notnull where TNext : notnull
    {
        if (schema is null) throw new ArgumentNullException(nameof(schema));

        return new ParseSchema<TIn, TNext>(schema, parse, message);
    }
}
