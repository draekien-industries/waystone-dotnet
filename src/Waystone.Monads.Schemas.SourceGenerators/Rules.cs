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

    private const string DocsRoot =
        "https://draekien-industries.wpei.me/source-generation/diagnostics#";

    private static DiagnosticDescriptor Create(
        string id,
        string title,
        string messageFormat,
        string description) =>
        new DiagnosticDescriptor(
            id,
            title,
            messageFormat,
            "Usage",
            DiagnosticSeverity.Error,
            true,
            description,
            DocsRoot + id.ToLowerInvariant());
}
