namespace Waystone.Monads.SourceGenerators.ErrorCodes;

using Microsoft.CodeAnalysis;

internal static class Rules
{
    public static readonly DiagnosticDescriptor FlagsEnum = new(
        "WMG0001",
        "An error code provider enum cannot be a flags enum",
        "'{0}' is marked with [ErrorCodeProvider] and [Flags], but a combined flags value has no single error code",
        "Usage",
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor AliasedValue = new(
        "WMG0002",
        "An error code provider enum cannot alias a value",
        "'{0}' and '{1}' share the value {2}, so neither has a single error code the generated members can return",
        "Usage",
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor ReservedMemberName = new(
        "WMG0003",
        "An error code provider member name collides with a generated type",
        "'{0}' declares a member named '{1}', which is also the name of a type generated into '{2}'",
        "Usage",
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor MissingErrorTypes = new(
        "WMG0004",
        "The Waystone.Monads error types are not resolvable",
        "'{0}' is marked with [ErrorCodeProvider] but '{1}' cannot be resolved in this compilation, so no error code members are generated",
        "Usage",
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor UnusableFormat = new(
        "WMG0005",
        "The error code format cannot be used",
        "The error code format for '{0}' cannot be used: {1}",
        "Usage",
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor FormatOmitsMember = new(
        "WMG0006",
        "The error code format does not distinguish members",
        "The error code format for '{0}' has no '{{member}}' placeholder, so every member would get the same code",
        "Usage",
        DiagnosticSeverity.Error,
        true);
}
