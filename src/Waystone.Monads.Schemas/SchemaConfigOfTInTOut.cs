namespace Waystone.Monads.Schemas;

using System.Collections.Generic;
using Waystone.Monads.Results;

/// <summary>The base class for a schema you declare as a set of fields.</summary>
/// <typeparam name="TIn">
/// The untrusted type being parsed, typically a request or a data transfer
/// object.
/// </typeparam>
/// <typeparam name="TOut">
/// The parsed type. Make it one a caller could not have constructed without
/// passing, so an unvalidated value is unrepresentable rather than merely
/// unlikely.
/// </typeparam>
/// <remarks>
/// <para>
/// Derive from this, mark the class <c>partial</c>, and override
/// <see cref="Configure" />. The generator then supplies <c>Instance</c> and the
/// <c>Schema.Fields</c> overload at the arity the body uses.
/// </para>
/// <para>
/// This is the only public way into the hierarchy.
/// <see cref="Schema{TIn,TOut}" /> itself cannot be derived from outside this
/// assembly, because the member that decides how violations accumulate is
/// internal — which is what keeps every schema's reporting behaviour identical.
/// Extend a schema through <c>Check</c>, <c>Transform</c> and <c>Not</c> rather
/// than by subclassing.
/// </para>
/// <para>
/// Separate from <see cref="Schema{TIn,TOut}" /> so that
/// <see cref="Configure" /> is declared only where it can be honoured. The
/// package's own internal nodes — the primitives, the combinators, the
/// decorators — define a parse by evaluating directly and have no field set to
/// configure, so they derive from <see cref="Schema{TIn,TOut}" /> and never
/// inherit a method they would have to refuse.
/// </para>
/// </remarks>
public abstract class SchemaConfig<TIn, TOut> : Schema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    /// <summary>Creates a schema declared as a set of fields.</summary>
    /// <remarks>
    /// Protected because the class exists to be derived from. Nothing needs to be
    /// passed to it; a schema holds no state beyond the rules its fields name.
    /// </remarks>
    protected SchemaConfig()
    {
    }

    /// <summary>Describes the parse as a set of fields.</summary>
    /// <param name="subject">The untrusted value being parsed.</param>
    /// <returns>
    /// The constructed value, or a <see cref="SchemaViolation" /> carrying the
    /// failures.
    /// </returns>
    /// <remarks>
    /// Called once per <see cref="Schema{TIn,TOut}.Parse" />. Build the body out of
    /// <c>Schema.Fields(...)</c> over the field constructors, finishing with
    /// <c>Into</c> to construct the result or <c>Checked</c> to gate without
    /// constructing one.
    /// </remarks>
    protected abstract Result<TOut, SchemaViolation> Configure(TIn subject);

    internal sealed override Outcome<TOut> Evaluate(
        TIn input,
        ParseContext context) =>
        Configure(input)
           .Match(
                context,
                static (value, _) => Outcome<TOut>.Passed(value),
                static (violation, inner) => Outcome<TOut>.Failed(
                    Rebase(violation.Violations, inner)));

    private static IReadOnlyList<Violation> Rebase(
        ViolationCollection violations,
        ParseContext context)
    {
        if (context.Path.IsRoot) return violations;

        var rebased = new Violation[violations.Count];

        for (var index = 0; index < violations.Count; index++)
        {
            rebased[index] = violations[index].Rebase(context.Path);
        }

        return rebased;
    }
}
