namespace Waystone.Monads.Schemas.SourceGenerators;

using Microsoft.CodeAnalysis;

internal static class Rules
{
    /// <summary>
    /// Reported against the type that is missing the modifier, which is not always
    /// the schema: a schema nested inside another type needs every type containing
    /// it to be partial too, and the one a reader has to edit is the one that is
    /// not.
    /// </summary>
    /// <remarks>
    /// The message says "generated members" rather than naming <c>Instance</c>, and
    /// deliberately. The constraint is the generator's, not that one member's, so the
    /// field-set ladder reports through this same rule rather than cloning it under a
    /// second id.
    /// </remarks>
    public static readonly DiagnosticDescriptor NotPartial = Create(
        "WMSC0001",
        "A generated schema must be declared partial",
        "'{0}' cannot receive its generated members because '{1}' is not declared partial; add the 'partial' modifier to '{1}'",
        "A source generator adds members through a second declaration of the same class, which the compiler accepts only where every type in the nesting chain is partial. A schema that is not partial is otherwise legal, so nothing else reports it and the schema simply gets nothing generated.");

    public static readonly DiagnosticDescriptor NoParameterlessConstructor =
        Create(
            "WMSC0002",
            "Do not hide a schema's parameterless constructor",
            "'{0}' has no accessible parameterless constructor, so its generated 'Instance' cannot be constructed; give it one, or take the values it needs from the input it parses",
            "The generated 'Instance' is a static property initialised with 'new'. 'SchemaConfig' supplies a protected parameterless constructor, so a derived schema inherits one until it declares a constructor of its own, at which point the implicit one disappears with no diagnostic of its own.");

    /// <summary>
    /// Covers all three names the generator writes into the schema, not just
    /// <c>Instance</c>. One rule rather than three, because the reader's problem and
    /// the reader's fix are the same in every case and the name is already a message
    /// argument.
    /// </summary>
    /// <remarks>
    /// <c>Schema</c> and <c>FieldSet</c> are only checked where a ladder is actually
    /// being emitted, so a schema that never calls <c>Schema.Fields</c> may keep a
    /// member of either name. Reporting them unconditionally would fail a build over
    /// a collision that does not exist.
    /// </remarks>
    public static readonly DiagnosticDescriptor NameAlreadyDeclared = Create(
        "WMSC0003",
        "Do not declare a member the generator emits",
        "'{0}' already declares a member named '{1}', which is a name the generator writes into this class; rename it, or remove it and use the generated one",
        "The generator reopens the class and emits 'Instance', a nested 'Schema' and a 'FieldSet' struct per field count into it, so a hand-written member of any of those names is a duplicate definition. Type parameters do not separate them: a nested type collides with an existing member of the same name whatever its arity. The compiler reports the collision against the generated file, which is not the file anyone can edit.");

    /// <summary>
    /// Reported at the <c>Into</c> call, not at the field list. The field list is
    /// what the author meant; the lambda is what disagrees with it.
    /// </summary>
    /// <remarks>
    /// The compiler already rejects this, as a delegate conversion failure against
    /// a generated type the author never wrote and cannot open. That message names
    /// neither the field count nor the file that decided it, so this rule exists to
    /// say the thing the reader needs rather than to catch something new.
    /// </remarks>
    public static readonly DiagnosticDescriptor IntoArityMismatch = Create(
        "WMSC0004",
        "Match the Into lambda to the number of fields",
        "'{0}' passes {1} fields to 'Schema.Fields' but its 'Into' lambda takes {2}; give the lambda one parameter per field, in the order the fields are listed",
        "The generated 'Into' takes one parameter per field, so a lambda of any other arity cannot bind to it. The arity is decided by the 'Schema.Fields' call rather than declared anywhere, which is what makes the compiler's own message hard to act on.");

    /// <summary>
    /// The one rule here that fires on code which compiles and runs, which is why
    /// it warns rather than fails.
    /// </summary>
    /// <remarks>
    /// Gating on a value without keeping it is legitimate — a confirmation field
    /// that must be a well-formed email but is never stored is the obvious case. So
    /// this cannot be an error without breaking correct code. It stays on because
    /// the other reading is the bug this package exists to prevent: a field that was
    /// validated, looks validated, and never reaches the object being built.
    /// </remarks>
    public static readonly DiagnosticDescriptor RefineDiscardsAValue = Advice(
        "WMSC0005",
        "Do not pass a value-producing field to Refine",
        "'{0}' yields '{1}' and 'Refine' discards it; list it in 'Schema.Fields' to reach the 'Into' lambda, or gate with 'Schema.Forbidden' or 'Schema.Extend' if the value is not wanted",
        "'Refine' takes the non-generic 'Field' base, which drops the value side, so it accepts any field and keeps only its violations. That is the right shape for a rule yielding 'Checked', which has nothing to contribute, and a silent mistake for one that parses a value somebody expected to find on the result.");

    /// <summary>
    /// Reported at the <c>CheckAsync</c> call, which is the one place a reader can
    /// act on: the schema holding it is fine, and so is every other rule in the
    /// chain.
    /// </summary>
    /// <remarks>
    /// An error rather than advice, even though nothing fails to generate. There is
    /// no reading of this code that works — a field set only ever runs the
    /// synchronous path, so the rule either throws or is skipped, and it never does
    /// its job. <c>CheckAsync</c> ships in the same release as this rule, so no
    /// existing code can be broken by failing the build on it.
    /// </remarks>
    public static readonly DiagnosticDescriptor AsyncRuleInAFieldSet = Create(
        "WMSC0006",
        "Do not reach an asynchronous rule from a field set",
        "'{0}' reaches an asynchronous rule from 'Configure', which only ever runs the synchronous path, so the rule throws rather than deciding anything; use 'Check' if the rule can answer from the value alone, or compose this schema outside a field set and parse it with 'ParseAsync'",
        "'SchemaConfig.Configure' returns a value rather than a task, so a field set evaluates synchronously even when the caller uses 'ParseAsync'. An asynchronous rule reached that way throws 'InvalidOperationException'. Nothing in the type system says so, because 'CheckAsync' returns the same schema type a synchronous rule does.");

    /// <summary>
    /// Reported at the call the generator did not recognise, which is the only place
    /// the reader can act on: the schema is fine and so is every other call in it.
    /// </summary>
    /// <remarks>
    /// Advice rather than an error, and the one rule here that warns about code that
    /// does not compile. The generator matches the receiver as written, so this fires
    /// on any unbound call to a member named <c>Fields</c> — including one that has
    /// nothing to do with a field set and failed to bind for its own reasons. The
    /// compiler is already reporting that call, so a second error would only add a
    /// build failure to a build that has one; a warning adds the explanation and
    /// stays wrong quietly.
    /// </remarks>
    public static readonly DiagnosticDescriptor FieldsNotRecognised = Advice(
        "WMSC0007",
        "Call Schema.Fields through the name Schema",
        "'{0}' spells its field-set call '{1}', which the generator matches by name rather than by binding it, so no ladder was generated; write the receiver as 'Schema', qualified by the type that contains it if you need to",
        "'Schema.Fields' is the member being generated, so it binds to nothing while the generator is deciding whether to emit it. The receiver therefore has to be recognised as written rather than resolved, and an alias, a renamed import or a call with no receiver at all carries nothing to recognise. Without this rule the only message is the compiler's, against a member the generator never created.");

    /// <summary>
    /// Reported at the argument the path was taken from, which is the expression the
    /// author would have to change if they did not want to name the field instead.
    /// </summary>
    /// <remarks>
    /// Advice, because the derived path is only usually wrong: an author who is not
    /// showing violations to anybody outside may well not care what
    /// <c>subject.Total.ToString()</c> reduces to. It stays on because the other
    /// reading is that the text of an expression the author wrote is now in an API
    /// response, and nothing else in the build says so.
    /// </remarks>
    public static readonly DiagnosticDescriptor FieldPathNotDerivable = Advice(
        "WMSC0008",
        "Name a field whose path cannot be read from its argument",
        "'{0}' takes this field's path from the expression itself, so a violation reports it as '{1}'; add '.Named(\"...\")' to report it under a name a caller can act on",
        "A field's path comes from 'CallerArgumentExpression', which hands the runtime the argument's source text and nothing else. A member access reduces to the member's name, which is the case the design is built around. Anything else — a method call, an indexer, a literal, a null-forgiving operator — keeps its punctuation, and that text then reaches logs and API responses alongside the violation.");

    private const string DocsRoot =
        "https://draekien-industries.wpei.me/source-generation/diagnostics#";

    /// <summary>
    /// A rule for a schema that cannot be generated. Failing the build is the whole
    /// point: the alternative is a missing member reported against a file the author
    /// cannot open.
    /// </summary>
    private static DiagnosticDescriptor Create(
        string id,
        string title,
        string messageFormat,
        string description) =>
        Descriptor(id, title, messageFormat, description, DiagnosticSeverity.Error);

    /// <summary>
    /// A rule for a schema that generates and runs, and is probably not what its
    /// author meant. It warns, because there is a reading of the same code that is
    /// correct and an error would leave that author no way forward but the rule's
    /// own id in an <c>.editorconfig</c>.
    /// </summary>
    private static DiagnosticDescriptor Advice(
        string id,
        string title,
        string messageFormat,
        string description) =>
        Descriptor(
            id,
            title,
            messageFormat,
            description,
            DiagnosticSeverity.Warning);

    private static DiagnosticDescriptor Descriptor(
        string id,
        string title,
        string messageFormat,
        string description,
        DiagnosticSeverity severity) =>
        new DiagnosticDescriptor(
            id,
            title,
            messageFormat,
            "Usage",
            severity,
            true,
            description,
            DocsRoot + id.ToLowerInvariant());
}
