namespace Waystone.SourceGenerators.AwaitedReceivers;

using Microsoft.CodeAnalysis;

internal static class Rules
{
    public static readonly DiagnosticDescriptor MustBePartial = new(
        "WSG0001",
        "A type marked for async receiver shapes must be partial",
        "'{0}' is marked with [GenerateAwaitedReceivers] but is not partial, so the generated receiver shapes cannot be added to it",
        "Usage",
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor UnknownMember = new(
        "WSG0002",
        "A generated member name matches nothing on the receiver type",
        "'{0}' has no public instance method named '{1}', so no awaited shape is generated for it",
        "Usage",
        DiagnosticSeverity.Error,
        true);
}
