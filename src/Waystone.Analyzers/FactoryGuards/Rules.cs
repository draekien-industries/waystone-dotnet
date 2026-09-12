namespace Waystone.Analyzers.FactoryGuards;

using Microsoft.CodeAnalysis;

internal static class Rules
{
    public static readonly DiagnosticDescriptor UnguardedFactoryReturn = new(
        "WA0001",
        "Guard a delegate-returned value against null",
        "'{0}' returns '{1}', which is guarded elsewhere by '{2}', so route this call through it rather than returning a null nobody can trace back here",
        "Reliability",
        DiagnosticSeverity.Error,
        true);
}
