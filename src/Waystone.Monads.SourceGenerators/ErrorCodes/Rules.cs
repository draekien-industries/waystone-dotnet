namespace Waystone.Monads.SourceGenerators.ErrorCodes;

using Microsoft.CodeAnalysis;

internal static class Rules
{
    public static readonly DiagnosticDescriptor FlagsEnum = Create(
        "WMG0001",
        "An error code catalog enum cannot be a flags enum",
        "'{0}' is marked with [ErrorCodeCatalog] and [Flags], but a combined flags value has no single error code");

    public static readonly DiagnosticDescriptor AliasedValue = Create(
        "WMG0002",
        "An error code catalog enum cannot alias a value",
        "'{0}' and '{1}' share the value {2}, so neither has a single error code the generated members can return");

    public static readonly DiagnosticDescriptor ReservedMemberName = Create(
        "WMG0003",
        "An error code catalog member name collides with a generated type",
        "'{0}' declares a member named '{1}', which is also the name of a type generated into '{2}'");

    public static readonly DiagnosticDescriptor MissingErrorTypes = Create(
        "WMG0004",
        "The Waystone.Monads error types are not resolvable",
        "'{0}' is marked with [ErrorCodeCatalog] but '{1}' cannot be resolved in this compilation, so no error code members are generated");

    public static readonly DiagnosticDescriptor UnusableFormat = Create(
        "WMG0005",
        "The error code format cannot be used",
        "The error code format for '{0}' cannot be used: {1}");

    public static readonly DiagnosticDescriptor FormatOmitsMember = Create(
        "WMG0006",
        "The error code format does not distinguish members",
        "The error code format for '{0}' has no '{{member}}' placeholder, so every member would get the same code");

    private const string DocsRoot =
        "https://draekien-industries.wpei.me/source-generation/diagnostics#";

    private static DiagnosticDescriptor Create(
        string id,
        string title,
        string messageFormat) =>
        new DiagnosticDescriptor(
            id,
            title,
            messageFormat,
            "Usage",
            DiagnosticSeverity.Error,
            true,
            null,
            DocsRoot + id.ToLowerInvariant());
}
