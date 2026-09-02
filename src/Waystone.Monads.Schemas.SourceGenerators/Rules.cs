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

    public static readonly DiagnosticDescriptor InstanceAlreadyDeclared = Create(
        "WMSC0003",
        "Do not declare a member named Instance on a schema",
        "'{0}' already declares a member named 'Instance', which is the name the generator emits; remove it and use the generated one",
        "The generator emits 'Instance' into a second declaration of the class, so a hand-written member of that name is a duplicate definition. The compiler reports the collision against the generated file, which is not the file anyone can edit.");

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
