namespace Waystone.Monads.Schemas;

using System.Collections.Generic;

/// <summary>A schema bound to one value, with the place that value came from.</summary>
/// <remarks>
/// <para>
/// Produced inside a <c>Configure</c> body by
/// <c>Schema.Required</c> and its siblings, passed straight to <c>Schema.Fields</c>, and never stored.
/// A schema is the reusable half; a field is the single-use half.
/// </para>
/// <para>
/// This non-generic base drops the value side, which is what <c>Refine</c>
/// accepts. A rule that gates without contributing a value therefore needs no
/// discard at the call site.
/// </para>
/// <para>
/// The hierarchy is closed. Write a custom rule with <c>Check</c>,
/// <c>Transform</c> or <c>Not</c> on a schema instead — a field subclass outside
/// this assembly could report failures the aggregator would not gather.
/// </para>
/// </remarks>
public abstract class Field
{
    /// <summary>Creates a field.</summary>
    /// <remarks>
    /// Reachable only from this assembly in practice: a derived type must also
    /// override an internal member it cannot see, so deriving from outside fails
    /// to compile.
    /// </remarks>
    protected Field()
    {
    }

    internal abstract void OnlyThisAssemblyMayDerive();

    internal abstract IReadOnlyList<Violation> Evaluate(ParseContext context);
}
