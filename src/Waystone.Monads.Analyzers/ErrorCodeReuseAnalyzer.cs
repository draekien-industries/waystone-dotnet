namespace Waystone.Monads.Analyzers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorCodeReuseAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.ErrorCodeReusedAcrossEnums);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols)
    {
        if (symbols.ErrorCodeProviderAttribute is null) return;

        var providers = new ConcurrentBag<ErrorCodeProviders.Provided>();

        string? assemblyFormat =
            ErrorCodeProviders.AssemblyFormat(context.Compilation);

        context.RegisterSymbolAction(
            symbol =>
            {
                foreach (ErrorCodeProviders.Provided provided in
                         ErrorCodeProviders.Collect(
                             (INamedTypeSymbol)symbol.Symbol,
                             symbols,
                             assemblyFormat))
                {
                    providers.Add(provided);
                }
            },
            SymbolKind.NamedType);

        context.RegisterCompilationEndAction(end => Report(end, providers));
    }

    private static void Report(
        CompilationAnalysisContext context,
        ConcurrentBag<ErrorCodeProviders.Provided> providers)
    {
        foreach (IGrouping<string, ErrorCodeProviders.Provided> collision in providers
                    .GroupBy(provided => provided.Code, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1))
        {
            List<ErrorCodeProviders.Provided> ordered = collision
               .OrderBy(
                    provided => provided.Member.ToDisplayString(),
                    StringComparer.Ordinal)
               .ToList();

            ErrorCodeProviders.Provided first = ordered[0];

            foreach (ErrorCodeProviders.Provided later in ordered.Skip(1))
            {
                if (SymbolEqualityComparer.Default.Equals(
                        first.Member.ContainingType,
                        later.Member.ContainingType))
                {
                    continue;
                }

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Rules.ErrorCodeReusedAcrossEnums,
                        later.Member.Locations.FirstOrDefault(
                            location => location.IsInSource),
                        first.Member.ToDisplayString(),
                        later.Member.ToDisplayString(),
                        collision.Key));
            }
        }
    }
}
