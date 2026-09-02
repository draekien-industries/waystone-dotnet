namespace Waystone.Monads.Schemas;

using System.Collections.Generic;

/// <summary>A field that contributes a parsed value to the constructed result.</summary>
/// <typeparam name="T">
/// What this field yields once it passes. <see cref="Checked" /> for a field that
/// only gates, and <c>Option&lt;T&gt;</c> for one produced by
/// <c>Schema.Optional</c>.
/// </typeparam>
/// <remarks>
/// The type argument is what fixes the parameter type of the <c>Into</c> lambda
/// at that position, so a schema that parses into a domain type hands the domain
/// type to the constructor rather than the raw input.
/// </remarks>
public abstract class Field<T> : Field where T : notnull
{
    /// <inheritdoc cref="Field()" />
    protected Field()
    {
    }

    internal abstract Outcome<T> EvaluateValue(ParseContext context);

    internal sealed override IReadOnlyList<Violation> Evaluate(
        ParseContext context) =>
        EvaluateValue(context).Violations;
}
