namespace Waystone.Monads.Schemas.SourceGenerators;

/// <summary>Everything the writer needs about one schema, reduced to values.</summary>
/// <remarks>
/// <para>
/// The transform resolves symbols and keeps none of them. A model held in the
/// incremental pipeline is compared for equality on every edit, and a symbol
/// compares by reference and roots the compilation it came from, so a pipeline
/// carrying one never hits its cache and never lets that compilation go.
/// </para>
/// <para>
/// It also lets the writer run <i>after</i> the language version is known, which
/// is what decides whether the emitted generic constraints can be spelled at all.
/// </para>
/// </remarks>
internal sealed record SchemaModel(
    string Namespace,
    EquatableArray<string> Containers,
    string Declaration,
    string QualifiedName,
    string Name,
    string Accessibility,
    EquatableArray<int> Arities);

/// <summary>What the generator decided about one schema.</summary>
/// <remarks>
/// A model and diagnostics together is a real case rather than a hypothetical one.
/// A schema whose <c>Into</c> lambda takes the wrong number of parameters still
/// gets its <c>Instance</c> and its ladder — withholding them would bury the one
/// message that explains the problem under a pile of unresolved names.
/// </remarks>
internal sealed record Analysis(
    string HintName,
    SchemaModel? Model,
    EquatableArray<DiagnosticInfo> Diagnostics)
{
    public static Analysis Failed(string hintName, DiagnosticInfo diagnostic) =>
        new Analysis(
            hintName,
            null,
            new EquatableArray<DiagnosticInfo>([diagnostic]));
}
