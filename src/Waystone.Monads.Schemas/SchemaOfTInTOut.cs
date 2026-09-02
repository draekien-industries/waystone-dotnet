namespace Waystone.Monads.Schemas;

using System;
using System.Threading;
using System.Threading.Tasks;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

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
/// Not derivable from outside this assembly, because the member deciding how
/// violations accumulate is internal. Derive from
/// <see cref="SchemaConfig{TIn,TOut}" /> to compose a schema out of fields.
/// Everything else is composition — the primitives, the combinators and the
/// refinements all return a schema, so a custom rule needs no subclass.
/// </para>
/// </remarks>
public abstract class Schema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    /// <summary>Creates a schema.</summary>
    /// <remarks>
    /// Reachable only from within this assembly in practice: evaluation is an
    /// internal abstract member, so a type declared outside it cannot satisfy the
    /// contract and the compiler refuses the subclass. That is what guarantees
    /// every schema accumulates failures the same way. Derive from
    /// <see cref="SchemaConfig{TIn,TOut}" /> instead.
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
    /// <see cref="ParseAsync" /> for those, which accepts a schema either way.
    /// Blocking on the rule instead would deadlock a caller on a synchronisation
    /// context and hide the mistake everywhere else.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// If the parse reaches a rule added by
    /// <see cref="CheckAsync(Func{TOut,CancellationToken,ValueTask{bool}},ViolationCode,string)" />.
    /// Reaching it depends on the input, since a rule after a failed conversion
    /// does not run — so a schema can pass this call for one input and throw for
    /// the next.
    /// </exception>
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
    /// <see cref="SchemaConfig{TIn,TOut}.Configure" /> — so mark that schema itself
    /// rather than the one holding it.
    /// </para>
    /// <para>
    /// Opt-in. Rendering the received value is the default, because it is the most
    /// useful token in the set and a silent <c>***</c> reads as a bug.
    /// </para>
    /// </remarks>
    public Schema<TIn, TOut> Sensitive() => new SensitiveSchema<TIn, TOut>(this);

    /// <summary>Adds a rule the parsed value has to satisfy.</summary>
    /// <param name="predicate">
    /// The rule. Returning false records a violation. Called once, only when
    /// everything before it produced a value, and expected to be free of side
    /// effects — a schema may run it on either the synchronous or the asynchronous
    /// path.
    /// </param>
    /// <param name="code">
    /// The kind of failure to report. Use the <see cref="ErrorCode" /> overload to
    /// report a code from your own domain instead.
    /// </param>
    /// <param name="message">
    /// What to tell a human. Supports <c>{Path}</c>, <c>{Received}</c> and
    /// <c>{Code}</c>, where <c>{Received}</c> is the value the rule rejected.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// A refinement, so the value survives the failure and every later rule on the
    /// chain still runs. That is what lets one parse report every problem at once.
    /// Contrast <c>Transform</c>, which produces no value when it fails.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="predicate" /> or <paramref name="message" /> is null.
    /// </exception>
    public Schema<TIn, TOut> Check(
        Func<TOut, bool> predicate,
        ViolationCode code,
        string message) =>
        Check(predicate, ViolationCodeCatalog.ToErrorCode(code), message);

    /// <summary>Adds a rule that reports a code of your own when it fails.</summary>
    /// <param name="predicate">
    /// The rule. Returning false records a violation. Called once, only when
    /// everything before it produced a value.
    /// </param>
    /// <param name="code">
    /// The code to report. Anywhere a <see cref="ViolationCode" /> is accepted an
    /// arbitrary <see cref="ErrorCode" /> is too, so a domain code such as
    /// <c>order.line_count_exceeded</c> groups through
    /// <see cref="ViolationCollection.ByCode" /> beside the built-in kinds.
    /// </param>
    /// <param name="message">
    /// What to tell a human. Supports <c>{Path}</c>, <c>{Received}</c> and
    /// <c>{Code}</c>.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>A refinement: the value survives, so later rules still run.</remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="predicate" />, <paramref name="code" /> or
    /// <paramref name="message" /> is null.
    /// </exception>
    public Schema<TIn, TOut> Check(
        Func<TOut, bool> predicate,
        ErrorCode code,
        string message) =>
        new CheckSchema<TIn, TOut>(this, predicate, code, message);

    /// <summary>Adds a rule that has to go somewhere to decide.</summary>
    /// <param name="predicate">
    /// The rule. Returning false records a violation. Called once, only when
    /// everything before it produced a value, and given the parse's cancellation
    /// token. Reach for it when deciding needs a database, a service or a file;
    /// a rule that can answer from the value alone belongs on
    /// <see cref="Check(Func{TOut,bool},ViolationCode,string)" />.
    /// </param>
    /// <param name="code">
    /// The kind of failure to report. Use the <see cref="ErrorCode" /> overload to
    /// report a code from your own domain instead.
    /// </param>
    /// <param name="message">
    /// What to tell a human. Supports <c>{Path}</c>, <c>{Received}</c> and
    /// <c>{Code}</c>, where <c>{Received}</c> is the value the rule rejected.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// <para>
    /// A refinement, like <c>Check</c>: the value survives the failure, so every
    /// later rule on the chain still runs and one parse still reports every problem
    /// at once.
    /// </para>
    /// <para>
    /// <b>The schema is asynchronous from here on, and
    /// <see cref="Parse" /> will throw on it.</b> Parse the result with
    /// <see cref="ParseAsync" />. This also rules the schema out of a
    /// <see cref="SchemaConfig{TIn,TOut}" /> field set, whose <c>Configure</c>
    /// returns a value rather than a task and so can only run the synchronous path
    /// — the generator reports <c>WMSC0006</c> where it can see that happening.
    /// </para>
    /// <para>
    /// The rule runs once per parse, in the order the chain declares. Nothing
    /// batches or deduplicates the calls, so a schema checking a hundred list items
    /// against a store makes a hundred round trips.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="predicate" /> or <paramref name="message" /> is null.
    /// </exception>
    public Schema<TIn, TOut> CheckAsync(
        Func<TOut, CancellationToken, ValueTask<bool>> predicate,
        ViolationCode code,
        string message) =>
        CheckAsync(predicate, ViolationCodeCatalog.ToErrorCode(code), message);

    /// <summary>Adds a rule that goes somewhere to decide and reports a code of your own.</summary>
    /// <param name="predicate">
    /// The rule. Returning false records a violation. Called once, only when
    /// everything before it produced a value, and given the parse's cancellation
    /// token.
    /// </param>
    /// <param name="code">
    /// The code to report. Anywhere a <see cref="ViolationCode" /> is accepted an
    /// arbitrary <see cref="ErrorCode" /> is too, so a domain code such as
    /// <c>order.sku_withdrawn</c> groups through
    /// <see cref="ViolationCollection.ByCode" /> beside the built-in kinds.
    /// </param>
    /// <param name="message">
    /// What to tell a human. Supports <c>{Path}</c>, <c>{Received}</c> and
    /// <c>{Code}</c>.
    /// </param>
    /// <returns>A schema that applies this rule after everything already on it.</returns>
    /// <remarks>
    /// A refinement: the value survives, so later rules still run. The schema is
    /// asynchronous from here on, so parse it with <see cref="ParseAsync" /> —
    /// <see cref="Parse" /> throws rather than blocking.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="predicate" />, <paramref name="code" /> or
    /// <paramref name="message" /> is null.
    /// </exception>
    public Schema<TIn, TOut> CheckAsync(
        Func<TOut, CancellationToken, ValueTask<bool>> predicate,
        ErrorCode code,
        string message) =>
        new AsyncCheckSchema<TIn, TOut>(this, predicate, code, message);

    /// <summary>Narrows the parsed value to a type that cannot fail to be built.</summary>
    /// <typeparam name="TNext">The type the schema produces from here on.</typeparam>
    /// <param name="convert">
    /// The conversion. Runs only when everything before it produced a value, and
    /// must not return null — a conversion that can fail belongs on the
    /// <see cref="Result{TOk,TErr}" /> overload.
    /// </param>
    /// <returns>A schema producing <typeparamref name="TNext" />.</returns>
    /// <remarks>
    /// Use this for a widening or a rename that no input can break, such as
    /// wrapping a validated string in a struct with no checks of its own. Reach for
    /// the <see cref="Result{TOk,TErr}" /> overload the moment the conversion can
    /// refuse.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="convert" /> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// If <paramref name="convert" /> returns null.
    /// </exception>
    public Schema<TIn, TNext> Transform<TNext>(Func<TOut, TNext> convert)
        where TNext : notnull =>
        new MapSchema<TIn, TOut, TNext>(this, convert);

    /// <summary>Narrows the parsed value through a conversion that may refuse it.</summary>
    /// <typeparam name="TNext">The type the schema produces from here on.</typeparam>
    /// <param name="convert">
    /// The conversion, typically a factory such as <c>EmailAddress.Create</c>. An
    /// <c>Err</c> becomes a violation carrying that error's own code and message,
    /// so a factory keeps its vocabulary. Runs only when everything before it
    /// produced a value.
    /// </param>
    /// <returns>A schema producing <typeparamref name="TNext" />.</returns>
    /// <remarks>
    /// <para>
    /// <b>This is the one seam in the promise that a parse reports every failure.</b>
    /// A refinement that fails leaves the value in place, so the rules after it
    /// still run and still report. A transform that fails produces no value, so
    /// nothing further along this chain has anything to look at and none of it
    /// runs. Sibling fields are unaffected and still report in full.
    /// </para>
    /// <para>
    /// So <c>Schema.Text.NotEmpty().MaxLength(5)</c> on <c>""</c> reports both
    /// failures, while <c>Schema.Text.Transform(Parse).Positive()</c> on
    /// <c>"abc"</c> reports only the transform. Put transforms last where you can.
    /// </para>
    /// <para>
    /// The error's message is rendered as a template, so a factory that returns
    /// <c>"Expected {Path} to be a currency code."</c> gets the path filled in.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="convert" /> is null.
    /// </exception>
    public Schema<TIn, TNext> Transform<TNext>(
        Func<TOut, Result<TNext, Error>> convert) where TNext : notnull =>
        new TransformSchema<TIn, TOut, TNext>(this, convert);

    /// <summary>Rejects a value that another schema would accept.</summary>
    /// <param name="rejected">
    /// The schema describing what is not allowed. It runs against the same input
    /// this one does, and its own violations are discarded — only whether it passed
    /// matters.
    /// </param>
    /// <param name="message">
    /// Why the value is not accepted. Required, because negation has no message
    /// worth deriving: <c>rejected</c> describes what was matched, not what was
    /// wanted. Supports <c>{Path}</c>, <c>{Received}</c> and <c>{Code}</c>.
    /// </param>
    /// <returns>A schema passing only when this one passes and the other does not.</returns>
    /// <remarks>
    /// A refinement rather than a combinator, and reports
    /// <c>schema_violation.not-allowed</c>. There is deliberately no
    /// <c>Schema.None</c>: it would have no derivable message, and
    /// <c>None(All(a, b))</c> means "not a, or not b" while nearly every reader
    /// takes it for "neither a nor b".
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="rejected" /> or <paramref name="message" /> is null.
    /// </exception>
    public Schema<TIn, TOut> Not(Schema<TIn, TOut> rejected, string message) =>
        new NotSchema<TIn, TOut>(this, rejected, message);

    /// <summary>Replaces the message on every failure this schema reports.</summary>
    /// <param name="message">
    /// The replacement, rendered per violation against that violation's own path
    /// and code. <c>{Received}</c> resolves to this schema's <i>input</i>, since a
    /// failure may mean no value was produced.
    /// </param>
    /// <returns>A schema reporting the same failures in your words.</returns>
    /// <remarks>
    /// Reaches every violation from every rule on the chain, not only the last one,
    /// so a chain of several checks collapses to one sentence. Apply it to a single
    /// rule instead by passing the message to that rule directly. Order matters
    /// against <see cref="WithCode(ViolationCode)" />: put <c>WithCode</c> first, so
    /// a message using <c>{Code}</c> renders the code you set.
    /// <para>
    /// <c>{Expected}</c> renders literally here, and always will. The bound belonged
    /// to one rule on the chain, and this message replaces the messages of all of
    /// them, so there is no single bound left to name. Restate it in the text.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="message" /> is null.
    /// </exception>
    public Schema<TIn, TOut> WithMessage(string message) =>
        new MessageSchema<TIn, TOut>(this, message);

    /// <summary>Replaces the code on every failure this schema reports.</summary>
    /// <param name="code">The kind of failure to report instead.</param>
    /// <returns>A schema reporting the same failures under one code.</returns>
    /// <remarks>
    /// Messages already rendered keep whatever <c>{Code}</c> gave them, because a
    /// message is rendered when its violation is created and the template is gone
    /// by now. Chain <see cref="WithMessage" /> after this call if the text names
    /// the code.
    /// </remarks>
    public Schema<TIn, TOut> WithCode(ViolationCode code) =>
        WithCode(ViolationCodeCatalog.ToErrorCode(code));

    /// <summary>Replaces the code on every failure with one of your own.</summary>
    /// <param name="code">
    /// The code to report instead, from your domain or from
    /// <c>ViolationCodeCatalog.Codes</c>.
    /// </param>
    /// <returns>A schema reporting the same failures under one code.</returns>
    /// <remarks>
    /// Messages already rendered keep whatever <c>{Code}</c> gave them. Chain
    /// <see cref="WithMessage" /> after this call if the text names the code.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="code" /> is null.
    /// </exception>
    public Schema<TIn, TOut> WithCode(ErrorCode code) =>
        new CodeSchema<TIn, TOut>(this, code);

    /// <summary>Reports this schema's failures under a name you choose.</summary>
    /// <param name="name">
    /// The path segment to report against, replacing the innermost one. Applied at
    /// the point of use — <c>Schema.Required(dto.Email, Email.Named("address"))</c>
    /// — because the schema itself is shared across every input that has its shape.
    /// </param>
    /// <returns>A schema reporting at the renamed path.</returns>
    /// <remarks>
    /// Overrides the segment a field constructor derived from its argument text.
    /// Reach for it when the property name is not what a caller should be shown, or
    /// when the argument was an expression rather than a member access.
    /// <para>
    /// It replaces a trailing <i>name</i> and appends after anything else, so a
    /// schema parsed directly through <see cref="Parse" /> reports at the name
    /// alone, and a branch of <c>Schema.Any</c> reports at <c>contact[1].name</c>
    /// rather than losing the branch number. Renaming an index is never what a
    /// caller means.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="name" /> is null.
    /// </exception>
    public Schema<TIn, TOut> Named(string name) =>
        new NamedSchema<TIn, TOut>(this, name);

    internal abstract Outcome<TOut> Evaluate(TIn input, ParseContext context);

    internal virtual ValueTask<Outcome<TOut>> EvaluateAsync(
        TIn input,
        ParseContext context,
        CancellationToken cancellationToken) =>
        new(Evaluate(input, context));
}
