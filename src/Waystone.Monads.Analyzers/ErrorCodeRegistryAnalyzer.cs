namespace Waystone.Monads.Analyzers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorCodeRegistryAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Rules.ErrorCodeMissingFromRegistry,
            Rules.StaleErrorCodeRegistryEntry);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols)
    {
        if (symbols.ErrorCodeProviderAttribute is null) return;

        AdditionalText? registry =
            ErrorCodeRegistry.Find(context.Options.AdditionalFiles);

        if (registry is null) return;

        SourceText? text = registry.GetText(context.CancellationToken);

        if (text is null) return;

        ImmutableArray<ErrorCodeRegistry.Entry> entries =
            ErrorCodeRegistry.Parse(text);

        var registered = new HashSet<string>(
            entries.Select(entry => entry.Code),
            StringComparer.Ordinal);

        var generated = new ConcurrentBag<string>();

        string? assemblyFormat =
            ErrorCodeProviders.AssemblyFormat(context.Compilation);

        context.RegisterSymbolAction(
            symbol => Inspect(symbol, symbols, assemblyFormat, registered, generated),
            SymbolKind.NamedType);

        context.RegisterCompilationEndAction(
            end => ReportStale(end, registry, entries, generated));
    }

    /// <summary>
    /// Reports every member of one enum whose code the registry does not list, and
    /// records what the enum generates for the end action.
    /// </summary>
    /// <remarks>
    /// This has to be a symbol action rather than part of the compilation end action
    /// below, even though both rules read the same set. A diagnostic reported from a
    /// compilation end action is a non-local diagnostic, and neither Roslyn's code fix
    /// service nor the analyzer testing library will offer a fix for one — so reporting
    /// WM2019 from the end action would leave a rule whose fix exists and is never
    /// reachable.
    /// </remarks>
    private static void Inspect(
        SymbolAnalysisContext context,
        MonadSymbols symbols,
        string? assemblyFormat,
        HashSet<string> registered,
        ConcurrentBag<string> generated)
    {
        foreach (ErrorCodeProviders.Provided provided in ErrorCodeProviders.Collect(
                     (INamedTypeSymbol)context.Symbol,
                     symbols,
                     assemblyFormat))
        {
            generated.Add(provided.Code);

            if (registered.Contains(provided.Code)) continue;

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rules.ErrorCodeMissingFromRegistry,
                    provided.Member.Locations.FirstOrDefault(
                        location => location.IsInSource),
                    provided.Member.ToDisplayString(),
                    provided.Code,
                    ErrorCodeRegistry.FileName));
        }
    }

    /// <summary>
    /// Reports every entry no enum in the compilation generates, which cannot be known
    /// until every enum has been seen.
    /// </summary>
    private static void ReportStale(
        CompilationAnalysisContext context,
        AdditionalText registry,
        ImmutableArray<ErrorCodeRegistry.Entry> entries,
        ConcurrentBag<string> generated)
    {
        var codes = new HashSet<string>(generated, StringComparer.Ordinal);

        foreach (ErrorCodeRegistry.Entry stale in entries)
        {
            if (codes.Contains(stale.Code)) continue;

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rules.StaleErrorCodeRegistryEntry,
                    Location.Create(registry.Path, stale.Span, stale.LineSpan),
                    stale.Code,
                    ErrorCodeRegistry.FileName));
        }
    }
}
