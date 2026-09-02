namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using Waystone.Monads.Results;

/// <summary>Runs the fields of one parse and gathers what they report.</summary>
/// <remarks>
/// <para>
/// The seam the generated <c>Schema.Fields</c> ladder is built on. Evaluating a
/// field is internal to this package, so generated code — which compiles in the
/// consuming assembly — reaches it through here and nowhere else. Nothing stops
/// you driving it by hand, and a schema whose field count is fixed and small can
/// reasonably do so, but the generated ladder is the shorter spelling.
/// </para>
/// <para>
/// One instance covers one parse: run every field, then read
/// <see cref="HasViolations" /> once. It is not thread safe and is not meant to
/// be: the fields of a single parse are run in the schema's declaration order,
/// which is the order their violations are reported in.
/// </para>
/// <para>
/// Every field runs, whatever the ones before it did. That is the whole point —
/// a caller gets every problem with their input at once rather than the first.
/// </para>
/// </remarks>
public sealed class FieldAccumulator
{
    private readonly List<Violation> _violations = new List<Violation>();

    private FieldAccumulator()
    {
    }

    /// <summary>Creates an accumulator for one parse.</summary>
    /// <returns>An accumulator holding no violations yet.</returns>
    /// <remarks>
    /// Named rather than constructed so that a parse reads as beginning
    /// somewhere. Do not reuse one across parses; violations gathered by the
    /// first would be reported by the second.
    /// </remarks>
    public static FieldAccumulator Start() => new FieldAccumulator();

    /// <summary>Reports whether anything has gone wrong so far.</summary>
    /// <value>
    /// True if any field run through this accumulator reported a violation;
    /// false otherwise.
    /// </value>
    /// <remarks>
    /// Check this after every field has run, not between them — a later field
    /// reports independently of an earlier one, which is the whole point. One
    /// violation anywhere fails the parse, including one left by a field that
    /// still produced a value.
    /// </remarks>
    public bool HasViolations => _violations.Count > 0;

    /// <summary>Runs a field and takes the value it yields.</summary>
    /// <typeparam name="T">What the field yields once it passes.</typeparam>
    /// <param name="field">
    /// The field to run, from <c>Schema.Required</c> or one of its siblings. Runs
    /// whether or not an earlier field failed.
    /// </param>
    /// <returns>
    /// The parsed value, or the default of <typeparamref name="T" /> when the
    /// field yielded none. A field that yields none has reported at least one
    /// violation, so <see cref="HasViolations" /> is the thing to branch on;
    /// reading this value in that case is meaningless.
    /// </returns>
    /// <remarks>
    /// A field can both produce a value and report violations — a refinement that
    /// failed leaves the value intact so the rest of its chain still runs. So a
    /// value coming back says nothing about whether the parse will succeed.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="field" /> is null.
    /// </exception>
    public T Take<T>(Field<T> field) where T : notnull
    {
        if (field is null) throw new ArgumentNullException(nameof(field));

        Outcome<T> outcome = field.EvaluateValue(ParseContext.Root);

        _violations.AddRange(outcome.Violations);

        return outcome.HasValue ? outcome.Value : default!;
    }

    /// <summary>Runs a field for its violations alone.</summary>
    /// <param name="field">
    /// The field to run. Takes the non-generic base, which drops the value side,
    /// so a rule that only gates needs no discard at the call site.
    /// </param>
    /// <remarks>
    /// For <c>Schema.Forbidden</c>, <c>Schema.Extend</c> and anything else whose
    /// job is to decide whether the parse may proceed rather than to contribute to
    /// what it builds.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="field" /> is null.
    /// </exception>
    public void Refine(Field field)
    {
        if (field is null) throw new ArgumentNullException(nameof(field));

        _violations.AddRange(field.Evaluate(ParseContext.Root));
    }

    /// <summary>Ends the parse with everything that went wrong.</summary>
    /// <typeparam name="TOut">
    /// The type the parse would have produced. Nothing of it is constructed; the
    /// parameter only shapes the result so a caller can return it directly.
    /// </typeparam>
    /// <returns>
    /// A <see cref="SchemaViolation" /> carrying every violation gathered so far,
    /// in the order the fields ran.
    /// </returns>
    /// <remarks>
    /// Call this only when <see cref="HasViolations" /> is true. The success half
    /// is deliberately absent: a caller that has the values in hand can construct
    /// its own result without handing this type a delegate to call, which is one
    /// closure allocation saved on every parse.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// If nothing has been reported, since there is no failure to describe.
    /// </exception>
    public Result<TOut, SchemaViolation> Failed<TOut>() where TOut : notnull
    {
        if (_violations.Count == 0)
        {
            throw new InvalidOperationException(
                "Nothing has been reported, so there is no failure to describe. Check HasViolations first.");
        }

        return Gather.ToFailure<TOut>(_violations.ToArray());
    }
}
