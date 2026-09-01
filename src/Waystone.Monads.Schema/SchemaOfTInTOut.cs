namespace Waystone.Monads.Schemas;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waystone.Monads.Results;

/// <summary>Turns an untrusted <typeparamref name="TIn" /> into a parsed <typeparamref name="TOut" />.</summary>
/// <typeparam name="TIn">
/// The shape that arrives — a DTO, a request body, a bare value.
/// </typeparam>
/// <typeparam name="TOut">
/// The shape that leaves. Often the same type, when the schema checks without
/// narrowing; a domain type when it parses into one.
/// </typeparam>
/// <remarks>
/// <para>
/// Holds no value, so declare one once as a static and reuse it against every
/// subject of that shape. Binding a schema to a value produces a
/// <see cref="Field" />, through <c>Schema.Required</c> and its siblings.
/// </para>
/// <para>
/// Every failure the parse can reach is reported at once. The single exception is
/// a failed transform, which produces no value for the rest of its own chain to
/// look at; its siblings are unaffected.
/// </para>
/// <para>
/// Derive from this to compose a schema out of fields, overriding
/// <see cref="Configure" />. Everything else is composition — the primitives,
/// the combinators and the refinements all return a schema, so a custom rule
/// needs no subclass.
/// </para>
/// </remarks>
public abstract class Schema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    /// <summary>Creates a schema.</summary>
    /// <remarks>
    /// Protected rather than internal so a consumer can derive a composed schema.
    /// The evaluation that gathers violations is internal, so a derived type
    /// outside this assembly can shape the parse through <see cref="Configure" />
    /// and cannot change how failures accumulate.
    /// </remarks>
    protected Schema()
    {
    }

    /// <summary>Parses an input, gathering every failure it finds.</summary>
    /// <param name="input">The untrusted value to parse.</param>
    /// <returns>
    /// The parsed value, or a <see cref="SchemaViolation" /> carrying every
    /// violation the parse reached.
    /// </returns>
    /// <remarks>
    /// Throws rather than blocks if the schema contains an asynchronous rule. Use
    /// <see cref="ParseAsync" /> for those.
    /// </remarks>
    public Result<TOut, SchemaViolation> Parse(TIn input) =>
        Evaluate(input, ParseContext.Root).ToResult();

    /// <summary>Parses an input asynchronously, gathering every failure it finds.</summary>
    /// <param name="input">The untrusted value to parse.</param>
    /// <param name="cancellationToken">
    /// Cancels the parse. Rules that do no asynchronous work ignore it.
    /// </param>
    /// <returns>
    /// The parsed value, or a <see cref="SchemaViolation" /> carrying every
    /// violation the parse reached.
    /// </returns>
    /// <remarks>
    /// Accepts a schema with no asynchronous rule in it and completes
    /// synchronously, so a caller that cannot tell either way can always use this
    /// one.
    /// </remarks>
    public async ValueTask<Result<TOut, SchemaViolation>> ParseAsync(
        TIn input,
        CancellationToken cancellationToken = default)
    {
        Outcome<TOut> outcome = await EvaluateAsync(
                input,
                ParseContext.Root,
                cancellationToken)
           .ConfigureAwait(false);

        return outcome.ToResult();
    }

    /// <summary>Marks the values this schema sees as unsafe to echo.</summary>
    /// <returns>
    /// A schema that behaves identically except that <c>{Received}</c> renders as
    /// <c>***</c> in every message it produces.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <c>{Received}</c> otherwise puts the rejected value into the message, which
    /// then reaches logs and API responses. Call this on any schema whose values
    /// are a password, a token, a government identifier or anything else that must
    /// not be repeated back.
    /// </para>
    /// <para>
    /// The marking reaches every rule this schema is built from, and every schema
    /// nested beneath it that is evaluated as part of it. It cannot reach a nested
    /// schema that renders its own messages first — one that overrides
    /// <see cref="Configure" /> — so mark that schema itself rather than the one
    /// holding it.
    /// </para>
    /// <para>
    /// Opt-in. Rendering the received value is the default, because it is the most
    /// useful token in the set and a silent <c>***</c> reads as a bug.
    /// </para>
    /// </remarks>
    public Schema<TIn, TOut> Sensitive() => new SensitiveSchema<TIn, TOut>(this);

    /// <summary>Describes the parse as a set of fields.</summary>
    /// <param name="subject">The untrusted value being parsed.</param>
    /// <returns>
    /// The constructed value, or a <see cref="SchemaViolation" /> carrying the
    /// failures.
    /// </returns>
    /// <remarks>
    /// Called once per <see cref="Parse" />. Build the body out of
    /// <c>Schema.Fields(...)</c> over the field constructors, finishing with
    /// <c>Into</c> to construct the result or <c>Checked</c> to gate without
    /// constructing one.
    /// </remarks>
    protected abstract Result<TOut, SchemaViolation> Configure(TIn subject);

    internal virtual Outcome<TOut> Evaluate(TIn input, ParseContext context) =>
        Configure(input)
           .Match(
                context,
                static (value, _) => Outcome<TOut>.Passed(value),
                static (violation, inner) => Outcome<TOut>.Failed(
                    Rebase(violation.Violations, inner)));

    internal virtual ValueTask<Outcome<TOut>> EvaluateAsync(
        TIn input,
        ParseContext context,
        CancellationToken cancellationToken) =>
        new(Evaluate(input, context));

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
