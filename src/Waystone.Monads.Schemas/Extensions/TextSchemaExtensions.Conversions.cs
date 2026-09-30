namespace Waystone.Monads.Schemas;

using System;

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
